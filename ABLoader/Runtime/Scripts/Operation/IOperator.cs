using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ILib.AssetBundles
{
	public interface ILoadOperator
	{
		Initializer Init();
		string GetCacheRoot();
		bool IsDownload(string name, string hash);
		string RequestUrl(string name, string hash);
		string LoadPath(string name, string hash);
		LoadOperation Load(string name, string hash);
	}
}
