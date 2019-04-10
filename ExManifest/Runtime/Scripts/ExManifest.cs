using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ILib.AssetBundles.ExManifest
{
	public class ExManifest : IBundleData
	{
		ABInfo[] m_Infos;
		Dictionary<string, ABInfo> m_Dic;
		DepInfo[] m_DepInfo;
		TagInfo[] m_TagInfo;
		Dictionary<string, RefInfo> m_Ref;
		string[] m_DepTemp = new string[16];
		HashSet<int> m_DepIndexTemp = new HashSet<int>();

		public ExManifest(ExManifestAsset manifestAsset)
		{
			m_Infos = manifestAsset.Infos;
			m_Dic = new Dictionary<string, ABInfo>(m_Infos.Length);
			foreach (var d in m_Infos)
			{
				m_Dic[d.Name] = d;
			}

			m_DepInfo = manifestAsset.DepInfo;
			m_TagInfo = manifestAsset.TagInfo;

			var reference = manifestAsset.RefInfo;
			m_Ref = new Dictionary<string, RefInfo>(reference.Length);
			foreach (var r in reference)
			{
				m_Ref[r.Id] = r;
			}
		}

		public int GetDepends(string name, out string[] deps)
		{
			ABInfo data;
			if (!m_Dic.TryGetValue(name, out data) && data.DepIndex >= 0)
			{
				deps = m_DepTemp;
				return 0;
			}
			var depInfo = m_DepInfo[data.DepIndex];
			for (int i = 0; i < depInfo.Deps.Length; i++)
			{
				m_DepTemp[i] = m_Infos[depInfo.Deps[i]].Name;
			}
			deps = m_DepTemp;
			return depInfo.Deps.Length;
		}

		public int GetAllDepends(string name, out string[] deps)
		{
			m_DepIndexTemp.Clear();
			int count = 0;
			foreach (var dep in GetDependImpls(name).Where(x => x != name))
			{
				if (count >= m_DepTemp.Length)
				{
					//1回目の負荷が大きいかもしれないが許容する
					Array.Resize(ref m_DepTemp, m_DepTemp.Length + 4);
				}
				m_DepTemp[count++] = dep;
			}
			deps = m_DepTemp;
			return count;
		}

		IEnumerable<string> GetDependImpls(string name)
		{
			ABInfo data;
			if (!m_Dic.TryGetValue(name, out data))
			{
				//データがねぇ
				yield break;
			}
			if (data.DepIndex < 0)
			{
				//依存もねぇ
				yield break;
			}
			if (!m_DepIndexTemp.Add(data.DepIndex))
			{
				//チェック済み
				yield break;
			}
			foreach (var dep in m_DepInfo[data.DepIndex].Deps)
			{
				var bundleName = m_Infos[dep].Name;
				yield return bundleName;
				foreach (var depName in GetDependImpls(bundleName))
				{
					yield return depName;
				}
			}
		}

		public string[] GetAllNames()
		{
			return m_Dic.Keys.ToArray();
		}

		public string GetHash(string name)
		{
			ABInfo data;
			if (m_Dic.TryGetValue(name, out data))
			{
				return data.Hash;
			}
			return name.GetHashCode().ToString();
		}

		public uint GetCRC(string name)
		{
			ABInfo data;
			if (m_Dic.TryGetValue(name, out data))
			{
				return data.CRC;
			}
			return 0;
		}

		public long GetSize(string name)
		{
			ABInfo data;
			if (m_Dic.TryGetValue(name, out data))
			{
				return data.Size;
			}
			return 1;
		}

		public (string bundleName, string assetName) GetReference(string id)
		{
			var _ref = m_Ref[id];
			return (m_Infos[_ref.Index].Name, _ref.AssetName);
		}

		public IEnumerable<string> GetBundleNames(string tag)
		{
			return m_Infos
				.Where(x => x.TagIndex >= 0)
				.Where(x => Array.IndexOf(m_TagInfo[x.TagIndex].Names, tag) >= 0)
				.Select(x => x.Name);
		}

	}
}
