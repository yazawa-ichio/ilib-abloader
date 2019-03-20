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

		public NetworkInitializer(string url, string hash)
		{
			m_Url = url;
			m_Hash = hash;
			m_CachePath = Cache.GetLoadPath(s_CacheName, m_Hash);
		}

		protected override void Start()
		{
			if (IsCache())
			{
				Load(m_CachePath);
			}
			else
			{
				Download(m_Url, s_CacheName, m_CachePath, () => Load(m_CachePath));
			}
		}

		bool IsCache()
		{
			return System.IO.File.Exists(m_CachePath);
		}

	}

}
