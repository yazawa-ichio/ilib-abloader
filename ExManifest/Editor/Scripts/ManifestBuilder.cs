using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ILib.AssetBundles.ExManifest
{

	public class ManifestBuilder
	{
		Setting m_Setting;
		ExManifestAsset m_Cache;
		AssetBundleManifest m_Current;

		public ITargetProvider TargetProvider = new TargetProvider();
		public IExDataProvider Extension;

		public ManifestBuilder(Setting setting)
		{
			m_Setting = setting;
		}

		public void Run()
		{
			//存在していれば前回のマニフェストのキャッシュを取得
			m_Cache = LoadCache();

			//今回ビルドするターゲット一覧を取得
			var builds = TargetProvider.GetBuildTargets(m_Cache);

			//通常のバンドルビルド
			if (!Directory.Exists(m_Setting.OutputPath))
			{
				Directory.CreateDirectory(m_Setting.OutputPath);
			}
			 m_Current = BuildPipeline.BuildAssetBundles(m_Setting.OutputPath, builds, m_Setting.Options, m_Setting.BuildTarget);

			//ビルド時にコンパイルが走りアセットがアンロードされるので再ロードする必要がある
			m_Cache = LoadCache();

			//一度、先ほどビルドしたマニフェストを退避させる
			var dirName = Path.GetFileName(m_Setting.OutputPath);
			string manifestBundlePath = Path.Combine(m_Setting.OutputPath, dirName);
			using (new StashFileScope(new string[] { manifestBundlePath, manifestBundlePath + ".manifest" }))
			{
				//拡張マニフェストをビルドする
				BuildManifest(builds);
			}
			//スコープを終えたので退避したマニフェストが戻る
		}

		ExManifestAsset LoadCache()
		{
			if (m_Setting.IgnoreCacheManifest)
			{
				return null;
			}
			var path = Path.Combine(m_Setting.OutputPath, m_Setting.ManifestBundleName);
			if (!File.Exists(path))
			{
				return null;
			}
			var bundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
			if (bundle == null)
			{
				return null;
			}
			var manifest = bundle.LoadAsset<ExManifestAsset>(m_Setting.ManifestAssetName);
			bundle.Unload(false);
			return manifest;
		}

		ExManifestAsset CreateManifest(AssetBundleBuild[] builds)
		{
			var manifestAsset = ScriptableObject.CreateInstance<ExManifestAsset>();
			
			//新しいビルドとキャッシュのデータをマージする
			Dictionary<string, DataEntry> entries = new Dictionary<string, DataEntry>();

			foreach (var build in builds)
			{
				entries[build.assetBundleName] = CreateEntry(build);
			}

			foreach (var cacheData in GetCacheData())
			{
				if (!entries.ContainsKey(cacheData.Info.Name))
				{
					entries[cacheData.Info.Name] = cacheData;
				}
			}

			// アセットの個々のインデックスとリファレンスデータを設定

			List<ABInfo> infoList = new List<ABInfo>();
			Dictionary<string, int> infoIndexDic = new Dictionary<string, int>();
			List<RefInfo> refList = new List<RefInfo>();
			foreach (var key in entries.Keys.OrderBy(x => x))
			{
				var entry = entries[key];
				var dataIndex = infoList.Count;
				infoIndexDic[entry.Info.Name] = dataIndex;
				infoList.Add(entry.Info);
				if (entry.Reference != null)
				{
					foreach (var refInfo in entry.Reference)
					{
						refInfo.Index = dataIndex;
						refList.Add(refInfo);
					}
				}
			}

			// アセットの依存とタグをインデックス指定で設定

			Dictionary<string, int> depIndexDic = new Dictionary<string, int>();
			List<DepInfo> depList = new List<DepInfo>();
			Dictionary<string, int> tagIndexDic = new Dictionary<string, int>();
			List<TagInfo> tagList = new List<TagInfo>();
			foreach (var key in entries.Keys.OrderBy(x => x))
			{
				var entry = entries[key];
				if (entry.Deps != null && entry.Deps.Length > 0)
				{
					var depKey = string.Join("@@", entry.Deps);
					if (!depIndexDic.ContainsKey(depKey))
					{
						var depIndex = depIndexDic.Count;
						depIndexDic[depKey] = depIndex;
						DepInfo depInfo = new DepInfo();
						depInfo.Deps = entry.Deps.Select(x => infoIndexDic[x]).ToArray();
						depList.Add(depInfo);
					}
					entry.Info.DepIndex = depIndexDic[depKey];
				}
				else
				{
					entry.Info.DepIndex = -1;
				}

				if (entry.Tags != null && entry.Tags.Length > 0)
				{
					var tagKey = string.Join("@@", entry.Tags);
					if (!tagIndexDic.ContainsKey(tagKey))
					{
						var tagIndex = tagIndexDic.Count;
						tagIndexDic[tagKey] = tagIndex;
						TagInfo tagInfo = new TagInfo();
						tagInfo.Names = entry.Tags;
						tagList.Add(tagInfo);
					}
					entry.Info.TagIndex = tagIndexDic[tagKey];
				}
				else
				{
					entry.Info.TagIndex = -1;
				}
			}

			manifestAsset.Infos = infoList.ToArray();
			manifestAsset.DepInfo = depList.ToArray();
			manifestAsset.TagInfo = tagList.ToArray();
			manifestAsset.RefInfo = refList.ToArray();

			return manifestAsset;
		}

		DataEntry CreateEntry(AssetBundleBuild build)
		{
			DataEntry entry = new DataEntry();
			string name = build.assetBundleName;
			string path = Path.Combine(m_Setting.OutputPath, name);
			entry.Info = CreateBundleData(name, path);
			entry.Deps = m_Current.GetDirectDependencies(name);
			Array.Sort(entry.Deps);
			if (Extension != null)
			{
				entry.Tags = Extension.GetTag(name, build.assetNames);
				Array.Sort(entry.Tags);
				entry.Reference = new List<RefInfo>(build.assetNames.Length);
				foreach (var assetPath in build.assetNames)
				{
					var id = Extension.GetReferenceId(name, assetPath);
					if (string.IsNullOrEmpty(id)) continue;
					RefInfo refInfo = new RefInfo();
					refInfo.AssetName = assetPath;
					refInfo.Id = id;
					entry.Reference.Add(refInfo);
				}
			}
			return entry;
		}

		ABInfo CreateBundleData(string name, string path)
		{
			ABInfo info = new ABInfo();
			info.Name = name;
			info.Hash = m_Current.GetAssetBundleHash(name).ToString();
			BuildPipeline.GetCRCForAssetBundle(path, out info.CRC);
			info.Size = new FileInfo(path).Length;
			return info;
		}

		IEnumerable<DataEntry> GetCacheData()
		{
			if (m_Cache == null) yield break;

			var refDic = GetRefDic();

			for (int index = 0; index < m_Cache.Infos.Length; index++)
			{
				ABInfo cacheInfo = m_Cache.Infos[index];
				if (!TargetProvider.IsTarget(cacheInfo.Name))
				{
					continue;
				}
				DataEntry entry = new DataEntry();
				entry.Info = cacheInfo.Duplicate();
				entry.Deps = GetDepNames(cacheInfo);
				entry.Tags = cacheInfo.TagIndex >= 0 ? m_Cache.TagInfo[cacheInfo.TagIndex].Names : Array.Empty<string>();
				refDic.TryGetValue(index, out entry.Reference);
				yield return entry;
			}
		}

		Dictionary<int, List<RefInfo>> GetRefDic()
		{
			Dictionary<int, List<RefInfo>> dic = new Dictionary<int, List<RefInfo>>();
			foreach (var refInfo in m_Cache.RefInfo)
			{
				List<RefInfo> refList;
				if (!dic.TryGetValue(refInfo.Index, out refList))
				{
					dic[refInfo.Index] = refList = new List<RefInfo>(1);
				}
				refList.Add(refInfo);
			}
			return dic;
		}

		string[] GetDepNames(ABInfo info)
		{
			if (info.DepIndex < 0)
			{
				return Array.Empty<string>();
			}
			var depInfo = m_Cache.DepInfo[info.DepIndex];
			string[] deps = new string[depInfo.Deps.Length];
			for (int i = 0; i < depInfo.Deps.Length; i++)
			{
				deps[i] = m_Cache.Infos[depInfo.Deps[i]].Name;
			}
			return deps;
		}

		void BuildManifest(AssetBundleBuild[] builds)
		{
			var dirName = Path.GetFileName(m_Setting.OutputPath);

			//前回のマニフェストと今回のマニフェストをマージする
			var exManifest = CreateManifest(builds);

			//アセットをビルド用にプロジェクト内に保存
			string manifestAssetDir = Path.Combine(m_Setting.ManifestAssetRootDir, dirName);
			string manifestAssetPath = Path.Combine(manifestAssetDir, m_Setting.ManifestAssetName + ".asset");
			if (!Directory.Exists(manifestAssetDir))
			{
				Directory.CreateDirectory(manifestAssetDir);
			}
			AssetDatabase.CreateAsset(exManifest, manifestAssetPath);

			//単体のバンドルとして拡張マニフェストをビルドする
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = m_Setting.ManifestBundleName;
			build.assetNames = new string[] { m_Setting.ManifestAssetRootDir };
			BuildPipeline.BuildAssetBundles(m_Setting.OutputPath, new AssetBundleBuild[] { build }, m_Setting.Options, m_Setting.BuildTarget);

			//拡張マニフェストを削除するべき？
		}

	}
}
