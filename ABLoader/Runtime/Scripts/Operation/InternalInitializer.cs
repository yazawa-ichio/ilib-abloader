using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	public class InternalInitializer : Initializer
	{
		string m_LoadPath;
		string m_AssetName;

		public InternalInitializer(string loadPath, string assetName = "AssetBundleManifest")
		{
			m_LoadPath = loadPath;
			m_AssetName = assetName;
		}

		protected override void Start()
		{
			Load(m_LoadPath, m_AssetName);
		}

	}
}