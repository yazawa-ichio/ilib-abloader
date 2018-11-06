using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ILib.AssetBundles
{
	//WebRequestとFile.IOを結び付けると至る所で例外を吐くのでtry-cache祭りを開催中

	internal class DownloadRequest : IRequest
	{
		public string name;
		public string url;
		public string cachePath;
		public Action onSuccess;
		public Action<Exception> onFail;

		UnityWebRequest m_webRequest;
		bool m_isSuccess;
		bool m_isFail;
		float m_progress;
		IRequestHander m_hander;

		public bool IsRunning => !m_isSuccess && !m_isFail;

		public string Name => name;

		public void SetHander(IRequestHander hander)
		{
			m_hander = hander;
		}

		public void DoStart()
		{
			try
			{
				SendImpl();
			}
			catch (System.Exception ex)
			{
				Fail(ex);
			}
		}

		public void DoAbort(Action onComplete)
		{
			try
			{
				m_webRequest?.Abort();
				m_webRequest = null;
				var error = Cache.TryDelete(name);
				if (error != null) ABLoader.LogError(error);
			}
			catch (System.Exception ex)
			{
				ABLoader.LogError(ex);
			}
			finally
			{
				onComplete();
			}
		}

		public void Dispose()
		{

		}

		void SendImpl()
		{
			var dir = Path.GetDirectoryName(cachePath);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			m_webRequest = UnityWebRequest.Get(url);
			var handler = new DownloadHandlerFile(cachePath);
			handler.removeFileOnAbort = true;
			m_webRequest.downloadHandler = handler;

			var op = m_webRequest.SendWebRequest();
			op.completed += (o) => OnComplete();
		}

		public void Abort()
		{
			try
			{
				m_webRequest?.Abort();
			}
			catch (System.Exception ex)
			{
				ABLoader.LogError(ex);
			}
		}

		void OnComplete()
		{
			m_hander?.OnComplete(this);
			if (m_webRequest == null) return;
			var error = m_webRequest.error;
			if (string.IsNullOrEmpty(error))
			{
				Success();
				m_webRequest.Dispose();
			}
			else
			{
				try
				{
					m_webRequest.Dispose();
				}
				finally
				{
					Fail(new System.Exception(error));
				}
			}
			m_webRequest = null;
		}

		void Success()
		{
			m_isSuccess = true;
			onSuccess?.Invoke();
			onSuccess = null;
		}

		void Fail(System.Exception ex)
		{
			m_isFail = true;
			Cache.TryDelete(name);
			onFail?.Invoke(ex);
			onFail = null;
		}

		float GetProgressImpl()
		{
			if (m_isSuccess) return 1f;
			if (m_isFail) return 0f;
			if (m_webRequest == null) return 0f;
			return m_webRequest.downloadProgress;
		}

		public float GetProgress()
		{
			var ret = GetProgressImpl();
			if (m_progress < ret) m_progress = ret;
			return ret;
		}

	}
}