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
		IBundleContainer m_container;
		bool m_disposed = false;
		public bool Disposed { get { return m_disposed; } }

		internal BundleContainerRef(IBundleContainer container)
		{
			m_container = container;
		}

		public void SetUnloadAll(bool unloadAll, bool depend = false)
		{
			m_container.SetUnloadAll(unloadAll, depend);
		}

		public void Dispose()
		{
			if (m_disposed)
			{
				return;
			}
			m_disposed = true;
			m_container.RemoveRef();
		}

		public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
		{
			if (m_disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			return m_container.LoadAsset<T>(assetName);
		}

		public void LoadAssetAsync<T>(string assetName, System.Action<T> onSuccess) where T : UnityEngine.Object
		{
			if (m_disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			m_container.LoadAssetAsync<T>(assetName, onSuccess);
		}

		public void LoadScene(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
		{
			if (m_disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			m_container.LoadScene(sceneName, mode);
		}

		public void LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode, System.Action onSuccess)
		{
			if (m_disposed) throw new System.InvalidOperationException("disposed bundle container ref");
			m_container.LoadSceneAsync(sceneName, mode, onSuccess);
		}

	}
}
