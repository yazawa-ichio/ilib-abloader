using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{

	public class NetworkInitializer : Initializer
	{
		static readonly string s_CacheName = "__BundleData";

		string m_Hash;
		string m_Url;
		string m_CachePath;
		string m_AssetName;

		public NetworkInitializer(string url, string hash, string assetName = "AssetBundleManifest")
		{
			m_Url = url;
			m_Hash = hash;
			m_CachePath = Cache.GetLoadPath(s_CacheName, m_Hash);
			m_AssetName = assetName;
		}

		protected override void Start()
		{
			if (IsCache())
			{
				Load(m_CachePath, m_AssetName);
			}
			else
			{
				Download(m_Url, s_CacheName, m_CachePath, () => Load(m_CachePath, m_AssetName));
			}
		}

		bool IsCache()
		{
			return System.IO.File.Exists(m_CachePath);
		}

	}

}