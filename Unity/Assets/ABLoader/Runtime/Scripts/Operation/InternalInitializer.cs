using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	public class InternalInitializer : Initializer
	{
		string m_loadPath;

		public InternalInitializer(string loadPath)
		{
			m_loadPath = loadPath;
		}

		protected override void Start()
		{
			Load(m_loadPath);
		}

	}
}