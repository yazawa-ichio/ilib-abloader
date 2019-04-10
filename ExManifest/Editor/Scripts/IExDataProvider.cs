using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles.ExManifest
{
	public interface IExDataProvider
	{
		string[] GetTag(string bundleName, string[] paths);
		string GetReferenceId(string bundleName, string path);
	}
}
