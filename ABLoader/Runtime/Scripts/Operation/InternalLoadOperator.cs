using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ILib.AssetBundles
{
	public class InternalLoadOperator : ILoadOperator
	{
		string m_Path;
		string m_Manifest;
		string m_ManifestAssetName;
		Queue<FileLoadOperation> m_OperationPool = new Queue<FileLoadOperation>();

		public InternalLoadOperator(string loadPath, string manifest, string manifestAssetName = "AssetBundleManifest")
		{
			m_Path = loadPath;
			m_Manifest = manifest;
			m_ManifestAssetName = manifestAssetName;
		}
		
		public Initializer Init()
		{
			var path = LoadPath(m_Manifest, "");
			return new InternalInitializer(path, m_ManifestAssetName);
		}

		public string GetCacheRoot()
		{
			throw new System.NotImplementedException();
		}

		public bool IsDownload(string name, string hash)
		{
			return false;
		}

		public string RequestUrl(string name, string hash)
		{
			throw new System.NotImplementedException();
		}

		public string LoadPath(string name, string hash)
		{
			return Path.Combine(m_Path, name);
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
			if (m_OperationPool.Count < ABLoader.MaxLoadCount * 2)
			{
				op.Reset();
				m_OperationPool.Enqueue(op as FileLoadOperation);
			}
		}

	}

}
