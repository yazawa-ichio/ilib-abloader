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
		BundleRef m_BundleRef;
		BundleRef[] m_Deps;
		bool m_Disposed;
		int m_RefCount;
		int m_DepLength;
		int m_DepCount;
		ABLoaderInstance m_Owner;
		bool m_Success;
		bool m_Error;
		List<Action<BundleContainerRef>> m_OnSuccess = new List<System.Action<BundleContainerRef>>(1);
		Action<Exception> m_OnFail;

		internal BundleContainer(string name, int depLength, ABLoaderInstance owner)
		{
			m_Owner = owner;
			Name = name;
			m_DepLength = depLength;
			if (depLength > 0)
			{
				m_Deps = new BundleRef[depLength];
			}
		}

		internal AssetBundle GetBundle()
		{
			return m_BundleRef.Bundle;
		}

		internal void SetEvent(Action<BundleContainerRef> onSuccess, Action<Exception> onFail)
		{
			if (m_Success)
			{
				TrySuccess(onSuccess);
				return;
			}
			else
			{
				if (onSuccess != null)
				{
					m_OnSuccess.Add(onSuccess);
				}
				m_OnFail += onFail;
			}
		}


		public void OnLoad(BundleRef bundle)
		{
			if (m_Error || m_Disposed)
			{
				return;
			}
			bundle.AddRef();
			if (Name == bundle.Name)
			{
				m_BundleRef = bundle;
			}
			else
			{
				m_Deps[m_DepCount++] = bundle;
			}
			//ロード済みかチェック
			if (m_BundleRef != null && m_DepLength == m_DepCount)
			{
				Success();
			}
		}

		void Success()
		{
			m_Success = true;
			if (m_OnSuccess.Count == 0)
			{
				Dispose();
				return;
			}
			using (CreateRef())
			{
				foreach (var onSuccess in m_OnSuccess)
				{
					TrySuccess(onSuccess);
				}
				m_OnSuccess.Clear();
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
				containerRef.Dispose();
				ABLoader.LogError(ex);
			}
		}

		public void OnFail(Exception ex)
		{
			if (m_Error)
			{
				return;
			}
			m_Error = true;
			Dispose();
			m_OnFail?.Invoke(ex);
		}

		public void SetUnloadAll(bool unloadAll, bool depend = false)
		{
			m_BundleRef.SetUnloadAll(unloadAll);
			if (depend && m_Deps != null)
			{
				for (int i = 0; i < m_Deps.Length; i++)
				{
					m_Deps[i].SetUnloadAll(unloadAll);
				}
			}
		}

		public void Dispose()
		{
			if (m_Disposed)
			{
				return;
			}
			m_Disposed = true;
			m_Owner.UnloadContainer(this);
			m_BundleRef?.RemoveRef();
			if (m_Deps != null)
			{
				for (int i = 0; i < m_Deps.Length; i++)
				{
					m_Deps[i]?.RemoveRef();
				}
			}
			m_BundleRef = null;
			m_Deps = null;
		}

		BundleContainerRef CreateRef()
		{
			m_RefCount++;
			return new BundleContainerRef(this);
		}

		void IBundleContainer.RemoveRef()
		{
			m_RefCount--;
			if (m_RefCount <= 0)
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
