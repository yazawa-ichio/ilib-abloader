using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ILib.AssetBundles.NameSetter
{

	public static class ABNameSetterTools
	{
		[MenuItem("Assets/Create/ILib/NameSetter", validate = true)]
		static bool CreateValidate()
		{
			var selected = Selection.activeObject;
			string path = AssetDatabase.GetAssetPath(selected);
			return AssetDatabase.IsValidFolder(path);
		}

		[MenuItem("Assets/Create/ILib/NameSetter")]
		static void Create()
		{
			var selected = Selection.activeObject;
			string path = AssetDatabase.GetAssetPath(selected);
			var dirName = System.IO.Path.GetFileName(path);
			path = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(path, dirName + "." + SetterAssetImporter.Ext));
			var asset = ScriptableObject.CreateInstance<SetterAsset>();
			Serializer.Serialize(asset, path);
			AssetDatabase.ImportAsset(path);
		}

	}

}