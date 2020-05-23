using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ILib.AssetBundles
{
	using Logger;

	//WebRequestとFile.IOを結び付けると至る所で例外を吐くのでtry-cache祭りを開催中

	internal class DownloadRequest : IRequest
	{
		public string Url;
		public string CachePath;
		public Action OnSuccess;
		public Action<Exception> OnFail;

		UnityWebRequest m_WebRequest;
		bool m_IsSuccess;
		bool m_IsFail;
		float m_Progress;
		IRequestHander m_Hander;

		public bool IsRunning => !m_IsSuccess && !m_IsFail;

		public string Name { get; set; }

		public void SetHander(IRequestHander hander)
		{
			m_Hander = hander;
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
				m_WebRequest?.Abort();
				m_WebRequest = null;
				var error = Cache.TryDelete(Name);
				if (error != null)
				{
					Log.Exception(error);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
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
			var dir = Path.GetDirectoryName(CachePath);
			if (!Directory.Exists(dir))
			{
				Log.Trace("[ilib-abloader]create directory {0}", dir);
				Directory.CreateDirectory(dir);
			}

			Log.Trace("[ilib-abloader] send web request {0}", Url);

			m_WebRequest = UnityWebRequest.Get(Url);
			var handler = new DownloadHandlerFile(CachePath);
			handler.removeFileOnAbort = true;
			m_WebRequest.downloadHandler = handler;

			var op = m_WebRequest.SendWebRequest();
			op.completed += (o) => OnComplete();

		}

		public void Abort()
		{
			try
			{
				m_WebRequest?.Abort();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		void OnComplete()
		{
			Log.Trace("[ilib-abloader] complete web request {0}", Url);
			m_Hander?.OnComplete(this);
			if (m_WebRequest == null) return;
			var error = m_WebRequest.error;
			if (string.IsNullOrEmpty(error))
			{
				Success();
				m_WebRequest.Dispose();
			}
			else
			{
				try
				{
					m_WebRequest.Dispose();
				}
				finally
				{
					Fail(new DownloadException(error, m_WebRequest));
				}
			}
			m_WebRequest = null;
		}

		void Success()
		{
			m_IsSuccess = true;
			OnSuccess?.Invoke();
			OnSuccess = null;
		}

		void Fail(Exception ex)
		{
			m_IsFail = true;
			Cache.TryDelete(Name);
			OnFail?.Invoke(ex);
			OnFail = null;
		}

		float GetProgressImpl()
		{
			if (m_IsSuccess) return 1f;
			if (m_IsFail) return 0f;
			if (m_WebRequest == null) return 0f;
			return m_WebRequest.downloadProgress;
		}

		public float GetProgress()
		{
			var ret = GetProgressImpl();
			if (m_Progress < ret) m_Progress = ret;
			return ret;
		}

	}
}