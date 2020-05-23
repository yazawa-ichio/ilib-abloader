using UnityEditor;
using System.Collections.Generic;

namespace ILib.AssetBundles.ExManifest
{
	public class TargetProvider : ITargetProvider
	{
		HashSet<string> Names;

		public bool IsTarget(string name)
		{
			if (Names == null)
			{
				Names = new HashSet<string>(AssetDatabase.GetAllAssetBundleNames());
			}
			return Names.Contains(name);
		}

		public AssetBundleBuild[] GetBuildTargets(ExManifestAsset cache)
		{
			AssetDatabase.RemoveUnusedAssetBundleNames();
			var names = AssetDatabase.GetAllAssetBundleNames();
			AssetBundleBuild[] builds = new AssetBundleBuild[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				string name = names[i];
				var build = new AssetBundleBuild();
				build.assetBundleName = name;
				build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name);
				builds[i] = build;
			}
			return builds;
		}

	}
}