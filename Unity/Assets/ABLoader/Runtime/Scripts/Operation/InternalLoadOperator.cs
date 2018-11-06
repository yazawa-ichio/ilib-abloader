using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ILib.AssetBundles
{
	public class InternalLoadOperator : ILoadOperator
	{
		string m_path;
		string m_manifest;

		public InternalLoadOperator(string loadPath, string manifest)
		{
			m_path = loadPath;
			m_manifest = manifest;
		}

		public Initializer Init()
		{
			var path = LoadPath(m_manifest, "");
			return new InternalInitializer(path);
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
			return Path.Combine(m_path, name);
		}

		public LoadOperation Load(string name, string hash)
		{
			return new FileLoadOperation();
		}

	}

}