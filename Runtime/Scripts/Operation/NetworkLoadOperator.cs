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

		public NetworkLoadOperator(string url, string cache, string manifest, string version)
		{
			m_Url = url;
			m_Cache = cache;
			m_Manifest = manifest;
			m_Version = version;
		}

		public Initializer Init()
		{
			var url = RequestUrl(m_Manifest, m_Version);
			return new NetworkInitializer(url, m_Version);
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
			return new FileLoadOperation();
		}

	}

}
