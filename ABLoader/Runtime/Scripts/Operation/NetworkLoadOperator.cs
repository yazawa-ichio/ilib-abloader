using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ILib.AssetBundles
{

	public class NetworkLoadOperator : ILoadOperator
	{
		string m_Url;
		string m_Cache;
		string m_Manifest;
		string m_Version;
		string m_ManifetAssetName;
		Queue<FileLoadOperation> m_OperationPool = new Queue<FileLoadOperation>();

		public NetworkLoadOperator(string url, string cache, string manifest, string version, string manifetAssetName = "AssetBundleManifest")
		{
			m_Url = url;
			m_Cache = cache;
			m_Manifest = manifest;
			m_Version = version;
			m_ManifetAssetName = manifetAssetName;
		}

		public Initializer Init()
		{
			var url = RequestUrl(m_Manifest, m_Version);
			return new NetworkInitializer(url, m_Version, m_ManifetAssetName);
		}

		public bool IsDownload(string name, string hash)
		{
			return true;
		}

		public string RequestUrl(string name, string hash)
		{
			return $"{m_Url}/{name}?hash={hash}";
		}

		public string GetCacheRoot()
		{
			return m_Cache;
		}

		public string LoadPath(string name, string hash)
		{
			//_∩(@_@)彡
			return Path.Combine(m_Cache, name + "@_@" + hash);
		}

		public LoadOperation Load(string name, string hash)
		{
			if (m_OperationPool.Count > 0)
			{
				return m_OperationPool.Dequeue();
			}
			return new FileLoadOperation();
		}

		public void CompleteLoad(LoadOperation op)
		{
			//最大ロード数の二倍はプールする
			if (m_OperationPool.Count < ABLoader.MaxLoadCount * 2)
			{
				op.Reset();
				m_OperationPool.Enqueue(op as FileLoadOperation);
			}
		}

	}

}