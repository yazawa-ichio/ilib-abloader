using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	/// <summary>
	/// 指定のアセットバンドルと依存先のバンドルをを保持したコンテナへの参照です。
	/// バンドルが不要になった際はDisposeを実行します。
	/// すべての参照がDisposeされた際に実際にアンロードされます。Disposeを忘れた場合リークします。
	/// </summary>
	public class BundleContainerRef : System.IDisposable
	{
		IBundleContainer m_Container;
		bool m_Disposed = false;
		public bool Disposed { get { return m_Disposed; } }

		internal BundleContainerRef(IBundleContainer container)
		{
			m_Container = container;
		}

		~BundleContainerRef()
		{
			if (ABLoader.UnloadMode != UnloadMode.Immediately)
			{
				Dispose();
			}
		}

		public void SetUnloadAll(bool unloadAll, bool depend = false)
		{
			m_Container.SetUnloadAll(unloadAll, depend);
		}

		public void Dispose()
		{
			if (m_Disposed)
			{
				return;
			}
			m_Disposed = true;
			m_Container.RemoveRef();
			System.GC.SuppressFinalize(this);
		}

		public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
		{
			if (m_Disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			return m_Container.LoadAsset<T>(assetName);
		}

		public void LoadAssetAsync<T>(string assetName, System.Action<T> onSuccess) where T : UnityEngine.Object
		{
			if (m_Disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			m_Container.LoadAssetAsync<T>(assetName, onSuccess);
		}

		public ContainerAssetLoading<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
		{
			if (m_Disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			var ret = new ContainerAssetLoading<T>();
			m_Container.LoadAssetAsync<T>(assetName, ret.SetResult);
			return ret;
		}

		public void LoadScene(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
		{
			if (m_Disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			m_Container.LoadScene(sceneName, mode);
		}

		public void LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode, System.Action onSuccess)
		{
			if (m_Disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			m_Container.LoadSceneAsync(sceneName, mode, onSuccess);
		}

		public ContainerSceneLoading LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
		{
			if (m_Disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			var ret = new ContainerSceneLoading();
			m_Container.LoadSceneAsync(sceneName, mode, ret.SetResult);
			return ret;
		}

	}
}
