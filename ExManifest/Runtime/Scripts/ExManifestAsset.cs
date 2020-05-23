using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ILib.AssetBundles.ExManifest
{
	public class ExManifestAsset : ScriptableObject, IBundleDataProvider
	{
		[SerializeField]
		public ABInfo[] Infos = Array.Empty<ABInfo>();

		[SerializeField]
		public DepInfo[] DepInfo = Array.Empty<DepInfo>();

		[SerializeField]
		public TagInfo[] TagInfo = Array.Empty<TagInfo>();

		[SerializeField]
		public RefInfo[] RefInfo = Array.Empty<RefInfo>();

		public IBundleData Provide()
		{
			return new ExManifest(this);
		}

	}
}