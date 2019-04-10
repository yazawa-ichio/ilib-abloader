using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ILib.AssetBundles.ExManifest.Tools
{
	public static class Build
	{

		[MenuItem("Tools/ExManifest/DefaultBuild")]
		static void DefaultBuid()
		{
			DefaultBuid(EditorUserBuildSettings.activeBuildTarget);
		}

		public static void DefaultBuid(BuildTarget target)
		{
			Setting config = Setting.CreateDefault(target);
			ManifestBuilder builder = new ManifestBuilder(config);
			builder.Extension = new ExDataProvider();
			builder.Run();
		}
	}
}
