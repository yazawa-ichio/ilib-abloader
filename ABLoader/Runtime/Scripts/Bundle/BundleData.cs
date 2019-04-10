using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	public interface IBundleData
	{
		string[] GetAllNames();
		string GetHash(string name);
		uint GetCRC(string name);
		int GetAllDepends(string name, out string[] deps);
		long GetSize(string name);
		(string bundleName, string assetName) GetReference(string id);
		IEnumerable<string> GetBundleNames(string tag);
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

		public uint GetCRC(string name)
		{
			return 0;
		}

		public int GetAllDepends(string name, out string[] deps)
		{
			deps = m_Manifest.GetAllDependencies(name);
			return deps.Length;
		}

		public long GetSize(string name)
		{
			return 1;
		}

		public (string bundleName, string assetName) GetReference(string id)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> GetBundleNames(string tag)
		{
			throw new System.NotImplementedException();
		}
	}
}
