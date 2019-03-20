using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ILib.AssetBundles
{

	public class NetworkLoadOperator : ILoadOperator
	{
		string m_url;
		string m_cache;
		string m_manifest;
		string m_version;

		public NetworkLoadOperator(string url, string cache, string manifest, string version)
		{
			m_url = url;
			m_cache = cache;
			m_manifest = manifest;
			m_version = version;
		}

		public Initializer Init()
		{
			var url = RequestUrl(m_manifest, m_version);
			return new NetworkInitializer(url, m_version);
		}

		public bool IsDownload(string name, string hash)
		{
			return true;
		}

		public string RequestUrl(string name, string hash)
		{
			return $"{m_url}/{name}?hash={hash}";
		}

		public string GetCacheRoot()
		{
			return m_cache;
		}

		public string LoadPath(string name, string hash)
		{
			//_∩(@_@)彡
			return Path.Combine(m_cache, name + "@_@" + hash);
		}

		public LoadOperation Load(string name, string hash)
		{
			return new FileLoadOperation();
		}

	}

}