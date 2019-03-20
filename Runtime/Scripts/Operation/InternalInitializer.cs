using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	public class InternalInitializer : Initializer
	{
		string m_LoadPath;

		public InternalInitializer(string loadPath)
		{
			m_LoadPath = loadPath;
		}

		protected override void Start()
		{
			Load(m_LoadPath);
		}

	}
}
