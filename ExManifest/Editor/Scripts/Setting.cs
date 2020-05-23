using UnityEngine;
using UnityEditor;

namespace ILib.AssetBundles.ExManifest
{
	public class Setting
	{
		public static readonly BuildAssetBundleOptions DefaultOptions = BuildAssetBundleOptions.ChunkBasedCompression;

		public string OutputPath;
		public string ManifestAssetName = "ExtensionManifestAsset";
		public string ManifestBundleName = "manifest";
		public string ManifestAssetRootDir = "Assets/Manifest";
		public bool IgnoreCacheManifest;
		public BuildTarget BuildTarget;
		public BuildAssetBundleOptions Options = DefaultOptions;

		public static Setting CreateDefault(BuildTarget target)
		{
			Setting setting = new Setting();
			setting.BuildTarget = target;
			setting.OutputPath = Application.dataPath.Replace("Assets", "AssetBundles/" + target.ToString());
			return setting;
		}

	}
}