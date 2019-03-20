using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	public interface IBundleData
	{
		string[] GetAllNames();
		string GetHash(string name);
		string[] GetAllDepends(string name);
		string[] GetDepends(string name);
		long GetSize(string name);
	}

	public class BundleData : IBundleData
	{
		static readonly Hash128 EmptyHash = new Hash128();

		AssetBundleManifest m_Manifest;
		public BundleData(AssetBundleManifest manifest)
		{
			m_Manifest = manifest;
		}

		public string[] GetAllNames()
		{
			return m_Manifest.GetAllAssetBundles();
		}

		public string GetHash(string name)
		{
			var hash = m_Manifest.GetAssetBundleHash(name);
			ABLoader.LogAssert(hash != EmptyHash);
			return hash.ToString();
		}

		public string[] GetAllDepends(string name)
		{
			return m_Manifest.GetAllDependencies(name);
		}

		public string[] GetDepends(string name)
		{
			return m_Manifest.GetDirectDependencies(name);
		}

		public long GetSize(string name)
		{
			return 1;
		}
	}
}
