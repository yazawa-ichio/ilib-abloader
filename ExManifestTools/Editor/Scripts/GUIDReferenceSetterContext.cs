using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace ILib.AssetBundles.ExManifest.Tools
{
	using NameSetter;
	using System.IO;

	public class GUIDReferenceSetterContext : SetterContext , IReferenceCollector
	{
		public override string Description => "ExManifetビルド時にGUIDを参照として登録します";

		public bool ApplyChildrenFolder = true;
		public string TargetExt = "*";

		public override void OnGUI()
		{
			string target = Directory + "/*";
			if (ApplyChildrenFolder)
			{
				target += "/*";
			}
			GUILayout.Label("対象ディレクトリ " + target);

			ApplyChildrenFolder = EditorGUILayout.Toggle("子のディレクトリにも適応します", ApplyChildrenFolder);

			using (new GUILayout.VerticalScope("box"))
			{
				EditorGUILayout.LabelField("対象の拡張子");
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				TargetExt = EditorGUILayout.TextField(nameof(TargetExt), TargetExt);
				GUILayout.Label("※「,」区切り or 「*」で全てを対象");
			}
		}

		public string GetId(string bundleName, string path)
		{

			if (!ApplyChildrenFolder && Path.GetDirectoryName(path) != Directory)
			{
				return "";
			}

			if (!Util.IsTargetExt(path, TargetExt))
			{
				return "";
			}

			return AssetDatabase.AssetPathToGUID(path);
		}

	}
}
