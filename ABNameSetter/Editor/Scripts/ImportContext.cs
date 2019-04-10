using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace ILib.AssetBundles.NameSetter
{

	public abstract class ImportContext<T> : SetterContext, IImportContext where T : ImportContext<T>
	{
		void IImportContext.Import(string path, AssetImporter importer)
		{
			Import(path, importer, SetterAssetCache.GetSetters<T>(path).Where(x => x != this));
		}

		protected abstract void Import(string path, AssetImporter importer, IEnumerable<T> children);

	}

}
