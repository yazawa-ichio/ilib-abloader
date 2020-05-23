using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ILib.AssetBundles.NameSetter
{
	public interface IImportContext
	{
		void Import(string path, AssetImporter importer);
	}
}