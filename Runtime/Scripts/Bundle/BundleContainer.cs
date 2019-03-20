using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	internal interface IBundleContainer
	{
		T LoadAsset<T>(string assetName) where T : UnityEngine.Object;

		void LoadAssetAsync<T>(string assetName, Action<T> onSuccess) where T : UnityEngine.Object;

		void LoadScene(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode);

		void LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode, Action onSuccess);

		void SetUnloadAll(bool unloadAll, bool depend = false);

		void RemoveRef();
	}

	internal class BundleContainer : IBundleContainer
	{

		public string Name { get; private set; }
		BundleRef m_bundleRef;
		BundleRef[] m_deps;
		bool m_disposed;
		int m_refCount;
		int m_depLength;
		int m_depCount;
		ABLoaderInstance m_owner;
		bool m_success;
		bool m_error;
		List<Action<BundleContainerRef>> m_onSuccess = new List<System.Action<BundleContainerRef>>(1);
		Action<Exception> m_onFail;

		internal BundleContainer(string name, int depLength, ABLoaderInstance owner)
		{
			m_owner = owner;
			Name = name;
			m_depLength = depLength;
			if (depLength > 0)
			{
				m_deps = new BundleRef[depLength];
			}
		}

		internal AssetBundle GetBundle()
		{
			return m_bundleRef.Bundle;
		}

		internal void SetEvent(Action<BundleContainerRef> onSuccess, Action<Exception> onFail)
		{
			if (m_success)
			{
				TrySuccess(onSuccess);
				return;
			}
			else
			{
				if (onSuccess != null)
				{
					m_onSuccess.Add(onSuccess);
				}
				m_onFail += onFail;
			}
		}


		public void OnLoad(BundleRef bundle)
		{
			if (m_error || m_disposed)
			{
				return;
			}
			bundle.AddRef();
			if (Name == bundle.Name)
			{
				m_bundleRef = bundle;
			}
			else
			{
				m_deps[m_depCount++] = bundle;
			}
			//ロード済みかチェック
			if (m_bundleRef != null && m_depLength == m_depCount)
			{
				Success();
			}
		}

		void Success()
		{
			m_success = true;
			if (m_onSuccess.Count == 0)
			{
				Dispose();
				return;
			}
			using (CreateRef())
			{
				foreach (var onSuccess in m_onSuccess)
				{
					TrySuccess(onSuccess);
				}
				m_onSuccess.Clear();
			}
		}

		void TrySuccess(Action<BundleContainerRef> onSuccess)
		{
			if (onSuccess == null) return;
			var containerRef = CreateRef();
			try
			{
				onSuccess(containerRef);
			}
			catch (Exception ex)
			{
				//例外を吐いた場合は即解放
				ABLoader.LogError(ex);
				containerRef.Dispose();
			}
		}

		public void OnFail(Exception ex)
		{
			if (m_error)
			{
				return;
			}
			m_error = true;
			Dispose();
			m_onFail?.Invoke(ex);
		}

		public void SetUnloadAll(bool unloadAll, bool depend = false)
		{
			m_bundleRef.SetUnloadAll(unloadAll);
			if (depend && m_deps != null)
			{
				for (int i = 0; i < m_deps.Length; i++)
				{
					m_deps[i].SetUnloadAll(unloadAll);
				}
			}
		}

		public void Dispose()
		{
			if (m_disposed)
			{
				return;
			}
			m_disposed = true;
			m_owner.UnloadContainer(this);
			m_bundleRef?.RemoveRef();
			if (m_deps != null)
			{
				for (int i = 0; i < m_deps.Length; i++)
				{
					m_deps[i]?.RemoveRef();
				}
			}
			m_bundleRef = null;
			m_deps = null;
		}

		BundleContainerRef CreateRef()
		{
			m_refCount++;
			return new BundleContainerRef(this);
		}

		void IBundleContainer.RemoveRef()
		{
			m_refCount--;
			if (m_refCount <= 0)
			{
				Dispose();
			}
		}

		public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
		{
			return GetBundle().LoadAsset<T>(assetName);
		}

		public void LoadAssetAsync<T>(string assetName, Action<T> onSuccess) where T : UnityEngine.Object
		{
			var op = GetBundle().LoadAssetAsync<T>(assetName);
			op.completed += (o) =>
			{
				onSuccess?.Invoke(op.asset as T);
			};
		}

		public void LoadScene(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, mode);
		}

		public void LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode, Action onSuccess)
		{
			var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
			op.completed += o => onSuccess?.Invoke();
		}

	}

}