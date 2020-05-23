using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace ILib.AssetBundles.NameSetter
{
	[ScriptedImporter(1, Ext)]
	public class SetterAssetImporter : ScriptedImporter
	{
		public const string Ext = "abNameSetter";

		public override void OnImportAsset(AssetImportContext ctx)
		{
			string text = System.IO.File.ReadAllText(ctx.assetPath);
			SetterAsset asset = ScriptableObject.CreateInstance<SetterAsset>();
			asset.m_XML = text;
			Serializer.Deserialize(asset, text);
			ctx.AddObjectToAsset("MainAsset", asset);
			ctx.SetMainObject(asset);
			SetterAssetCache.SetDirty();
		}
	}

	[CustomEditor(typeof(SetterAssetImporter))]
	public class SetterAssetImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			//RevertとApplyボタンを表示、処理も実装
			this.ApplyRevertGUI();
		}

		protected override bool OnApplyRevertGUI()
		{
			if (GUILayout.Button("Revert"))
			{
				var importer = target as SetterAssetImporter;
				AssetDatabase.LoadAssetAtPath<SetterAsset>(importer.assetPath).Load();
				return true;
			}
			if (GUILayout.Button("Apply"))
			{
				var importer = target as SetterAssetImporter;
				AssetDatabase.LoadAssetAtPath<SetterAsset>(importer.assetPath).Save();
				return true;
			}
			return false;
		}

	}

}