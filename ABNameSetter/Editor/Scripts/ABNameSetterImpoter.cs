using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ILib.AssetBundles.NameSetter
{
	public class ABNameSetterImpoter : AssetPostprocessor
	{
		void OnPreprocessAsset()
		{
			string path = assetImporter.assetPath;
			if (!System.IO.Path.HasExtension(path))
			{
				return;
			}
			if (!path.StartsWith("Assets"))
			{
				return;
			}
			foreach (var root in SetterAssetCache.GetRootSetters<IImportContext>(path))
			{
				root.Import(path, assetImporter);
			}
		}
	}
}