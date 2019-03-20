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

		public InternalLoadOperator(string loadPath, string manifest)
		{
			m_Path = loadPath;
			m_Manifest = manifest;
		}
		
		public Initializer Init()
		{
			var path = LoadPath(m_Manifest, "");
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
			return Path.Combine(m_Path, name);
		}

		public LoadOperation Load(string name, string hash)
		{
			return new FileLoadOperation();
		}

	}

}
