using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles.NameSetter
{
	public abstract class SetterContext
	{
		public abstract string Description { get; }
		string m_Directory;
		public string Directory => m_Directory;
		public void SetDirectory(string directory) => m_Directory = directory;
		public abstract void OnGUI();
	}
}
