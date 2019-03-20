using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{

	public class NetworkInitializer : Initializer
	{
		static readonly string s_cacheName = "__BundleData";

		string m_hash;
		string m_url;
		string m_cachePath;

		public NetworkInitializer(string url, string hash)
		{
			m_url = url;
			m_hash = hash;
			m_cachePath = Cache.GetLoadPath(s_cacheName, m_hash);
		}

		protected override void Start()
		{
			if (IsCache())
			{
				Load(m_cachePath);
			}
			else
			{
				Download(m_url, s_cacheName, m_cachePath, () => Load(m_cachePath));
			}
		}

		bool IsCache()
		{
			return System.IO.File.Exists(m_cachePath);
		}

	}

}