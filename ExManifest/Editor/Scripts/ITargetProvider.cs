using UnityEditor;

namespace ILib.AssetBundles.ExManifest
{
	public interface ITargetProvider
	{
		bool IsTarget(string name);
		AssetBundleBuild[] GetBuildTargets(ExManifestAsset cache);
	}
}
