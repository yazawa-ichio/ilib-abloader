using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEditor;

namespace ILib.AssetBundles.ExManifest.Tools
{
	using NameSetter;

	public class PathToTagSetterContext : SetterContext , ITagCollector
	{
		public override string Description => "ExManifetビルド時にタグを設定します";

		public enum TagMode
		{
			DirName,
			InputName,
		}

		static readonly string[] s_ModeDescription = {
			"対象のディレクトリ名をタグにします",
			"対象の入力したタグにします",
		};

		public bool ApplyChildrenFolder = true;
		public TagMode NameMode = TagMode.DirName;
		public string InputName;
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
				EditorGUILayout.LabelField("タグ名の決定方法");
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				NameMode = (TagMode)EditorGUILayout.Popup(nameof(NameMode), (int)NameMode, s_ModeDescription);
			}

			if (NameMode == TagMode.InputName)
			{
				using (new GUILayout.VerticalScope("box"))
				{
					EditorGUILayout.LabelField("タグ名");
					GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
					InputName = EditorGUILayout.TextField(nameof(InputName), InputName);
				}
			}

			using (new GUILayout.VerticalScope("box"))
			{
				EditorGUILayout.LabelField("対象の拡張子");
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				TargetExt = EditorGUILayout.TextField(nameof(TargetExt), TargetExt);
				GUILayout.Label("※「,」区切り or 「*」で全てを対象");
			}
		}

		public IEnumerable<string> GetTags(string bundleName, string[] paths)
		{
			foreach (var path in paths)
			{
				if (!ApplyChildrenFolder && Path.GetDirectoryName(path) != Directory)
				{
					continue;
				}
				if (!Util.IsTargetExt(path, TargetExt))
				{
					continue;
				}
				string tag = "";
				switch (NameMode)
				{
					case TagMode.DirName:
						tag = Path.GetFileName(Path.GetDirectoryName(path));
						break;
					case TagMode.InputName:
						tag = InputName;
						break;
				}
				if (!string.IsNullOrEmpty(tag))
				{
					yield return tag;
				}
			}
		}
	}
}
