using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	using Logger;

	internal class BundleRef : System.IDisposable
	{
		ABLoaderInstance m_Owner;
		AssetBundle m_Bundle;
		int m_Count;
		bool m_UnloadAll;
		bool m_Disposed;

		public string Name { get; private set; }
		public AssetBundle Bundle { get { return m_Bundle; } }
		public bool HasRef { get { return m_Count > 0; } }

		public BundleRef(ABLoaderInstance owner, string name, AssetBundle bundle)
		{
			m_Owner = owner;
			Name = name;
			m_Bundle = bundle;
			Log.Trace("[ilib-abloader] create BundleRef {0}.", Name);
		}

		public void AddRef()
		{
			m_Count++;
		}

		public void RemoveRef()
		{
			m_Count--;
			if (m_Count <= 0 && !m_Disposed)
			{
				m_Owner.UnloadRef(this);
			}
		}

		public void SetUnloadAll(bool unloadAll)
		{
			Log.Trace("[ilib-abloader] Name {0}, set unloadAll : {1}.", Name, unloadAll);
			m_UnloadAll = unloadAll;
		}

		public void Dispose()
		{
			Log.Trace("[ilib-abloader] unload AssetBundle Name {0}.", Name);
			m_Disposed = true;
			if (m_Bundle != null)
			{
				m_Bundle.Unload(m_UnloadAll);
			}
			m_Bundle = null;
		}
	}
}
