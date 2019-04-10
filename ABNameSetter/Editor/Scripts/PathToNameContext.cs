using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ILib.AssetBundles.NameSetter
{

	public class PathToNameContext : ImportContext<PathToNameContext>
	{
		public enum Mode
		{
			DirName,
			AssetName,
			AssetNameWithExt,
		}

		static readonly string[] s_ModeDescription = {
			"ディレクトリ名をバンドル名にします",
			"アセット名をバンドル名に設定します",
			"アセット名と拡張子をバンドル名に設定します",
		};

		public override string Description => "パスをアセットバンドル名として設定します";

		public bool ApplyChildrenFolder = true;

		public bool OverrideStartDirPath = false;
		public bool UseStartDirPathAsAssetPath = true;
		public string StartDirPathStr = "";

		public bool OverrideNameMode = true;
		public Mode NameMode = Mode.DirName;
		public bool OverrideTargetExt = false;
		public string TargetExt = "*";
		public bool OverrideBundleExt = false;
		public string BundleExt = "bundle";

		string GetStartDirPath()
		{
			if (UseStartDirPathAsAssetPath)
			{
				return Directory;
			}
			else
			{
				return StartDirPathStr;
			}
		}

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
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("バンドル名として開始するパス", GUILayout.ExpandWidth(true));
					OverrideStartDirPath = EditorGUILayout.ToggleLeft("親を書き換えるか？", OverrideStartDirPath);
				}
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				UseStartDirPathAsAssetPath = EditorGUILayout.Toggle("アセットのパスを使用します", UseStartDirPathAsAssetPath);
				using (new EditorGUI.DisabledGroupScope(UseStartDirPathAsAssetPath))
				{
					if (UseStartDirPathAsAssetPath)
					{
						EditorGUILayout.TextField(nameof(StartDirPathStr), Directory);
					}
					else
					{
						StartDirPathStr = EditorGUILayout.TextField(nameof(StartDirPathStr), StartDirPathStr);
					}
				}
			}

			using (new GUILayout.VerticalScope("box"))
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("名前の決定方法", GUILayout.ExpandWidth(true));
					OverrideNameMode = EditorGUILayout.ToggleLeft("親を書き換えるか？", OverrideNameMode);
				}
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				NameMode = (Mode)EditorGUILayout.Popup(nameof(NameMode), (int)NameMode, s_ModeDescription);
			}

			using (new GUILayout.VerticalScope("box"))
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("対象の拡張子", GUILayout.ExpandWidth(true));
					OverrideTargetExt = EditorGUILayout.ToggleLeft("親を書き換えるか？", OverrideTargetExt);
				}
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				TargetExt = EditorGUILayout.TextField(nameof(TargetExt), TargetExt);
				GUILayout.Label("※「,」区切り or 「*」で全てを対象");
			}

			using (new GUILayout.VerticalScope("box"))
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("バンドルの拡張子", GUILayout.ExpandWidth(true));
					OverrideBundleExt = EditorGUILayout.ToggleLeft("親を書き換えるか？", OverrideBundleExt);
				}
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				BundleExt = EditorGUILayout.TextField(nameof(BundleExt), BundleExt);
			}
		}

		protected override void Import(string path, AssetImporter importer, IEnumerable<PathToNameContext> children)
		{
			string startDirPath = GetStartDirPath();
			Mode nameMode = NameMode;
			string targetExt = TargetExt;
			string bundleExt = BundleExt;
			foreach (var ctx in children)
			{
				if (ctx.OverrideStartDirPath) startDirPath = ctx.GetStartDirPath();
				if (ctx.OverrideNameMode) nameMode = ctx.NameMode;
				if (ctx.OverrideTargetExt) targetExt = ctx.TargetExt;
				if (ctx.OverrideBundleExt) bundleExt = ctx.BundleExt;
			}

			if (!ApplyChildrenFolder && Path.GetDirectoryName(path) != Directory)
			{
				return;
			}

			if (!Util.IsTargetExt(path, targetExt))
			{
				return;
			}

			if (!string.IsNullOrEmpty(startDirPath) && startDirPath[startDirPath.Length-1] != '/')
			{
				startDirPath += "/";
			}
			path = path.Replace(startDirPath, "");
			string bundleName = "";
			switch (nameMode)
			{
				case Mode.DirName:
					bundleName = Path.GetDirectoryName(path);
					break;
				case Mode.AssetName:
					bundleName = path.Substring(0, path.Length - Path.GetExtension(path).Length);
					break;
				case Mode.AssetNameWithExt:
					string ext = Path.GetExtension(path);
					bundleName = path.Substring(0, path.Length - ext.Length) + "__" + ext.Substring(1);
					break;
			}
			if (!string.IsNullOrEmpty(bundleExt))
			{
				bundleName += "." + bundleExt;
			}

			bundleName = bundleName.ToLower();

			if (importer.assetBundleName != bundleName)
			{
				importer.assetBundleName = bundleName;
			}
		}
	}

}
