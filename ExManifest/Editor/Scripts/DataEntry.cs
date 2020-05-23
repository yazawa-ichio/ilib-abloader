using System.Collections.Generic;
using UnityEditor;


namespace ILib.AssetBundles.ExManifest
{
	public class DataEntry
	{
		public ABInfo Info;
		public string[] Deps;
		public string[] Tags;
		public List<RefInfo> Reference;
	}
}