using System.Collections.Generic;

namespace ILib.AssetBundles.ExManifest.Tools
{
	public interface ITagCollector
	{
		IEnumerable<string> GetTags(string bundleName, string[] paths);
	}
}
