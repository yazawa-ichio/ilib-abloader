using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	internal class BundleRef
	{
		ABLoaderInstance m_owner;
		AssetBundle m_bundle;
		int m_count;
		bool m_unloadAll;

		public string Name { get; private set; }
		public AssetBundle Bundle { get { return m_bundle; } }

		public BundleRef(ABLoaderInstance owner, string name, AssetBundle bundle)
		{
			m_owner = owner;
			Name = name;
			m_bundle = bundle;
		}

		public void AddRef()
		{
			m_count++;
		}

		public void RemoveRef()
		{
			m_count--;
			if (m_count <= 0 && m_bundle != null)
			{
				m_bundle.Unload(m_unloadAll);
				m_bundle = null;
				m_owner.UnloadRef(this);
			}
		}

		public void SetUnloadAll(bool unloadAll)
		{
			m_unloadAll = unloadAll;
		}

		public void Unload()
		{
			m_bundle?.Unload(m_unloadAll);
			m_bundle = null;
		}

	}
}