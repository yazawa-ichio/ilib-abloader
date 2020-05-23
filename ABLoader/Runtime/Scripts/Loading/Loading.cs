using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ILib.AssetBundles
{
	public interface ILoading : IDisposable, IEnumerator
	{
		string BundleName { get; }

		string AssetName { get; }

		bool IgnoreError { get; set; }

		bool ForceAwaiterCompleteIfError { get; set; }

		int MaxRetryCount { get; set; }

		Action<ILoading, Exception, Action<bool>> RetryHandle { get; set; }

		Action<Exception> ErrorHandle { get; set; }

		Exception Error { get; }

		bool IsCompleted { get; }

		bool Disposed { get; }

		bool DoRequested { get; }

		void TryRequest();
	}

	public abstract class Loading<T> : ILoading
	{
		public string BundleName { get; private set; }

		public string AssetName { get; private set; }

		public bool IgnoreError { get; set; }

		public bool ForceAwaiterCompleteIfError { get; set; }

		public int MaxRetryCount { get; set; } = 5;

		public Action<ILoading, Exception, Action<bool>> RetryHandle { get; set; }

		public Action<Exception> ErrorHandle { get; set; }

		public T Result { get; private set; }

		public Exception Error { get; private set; }

		public bool IsCompleted
		{
			get
			{
				TryRequest();
				return m_Success || Error != null || Disposed;
			}
		}

		public bool Disposed { get; private set; }

		public bool DoRequested { get; private set; }

		bool m_Success;
		Action<T> m_Load;
		Action m_AwaiterComplete;
		int m_RetryCount;

		protected Loading(string bundleName, string assetName)
		{
			BundleName = bundleName;
			AssetName = assetName;
		}

		public void SetConfig(LoadingConfig config)
		{
			MaxRetryCount = config.MaxRetryCount;
			IgnoreError = config.IgnoreError;
			RetryHandle = config.RetryHandle;
			ErrorHandle = config.ErrorHandle;
		}

		public void Load(Action<T> action)
		{
			if (Disposed) throw new ObjectDisposedException($"{ GetType().Name }:{BundleName}:{AssetName}");
			TryRequest();
			if (m_Success)
			{
				action?.Invoke(Result);
			}
			else
			{
				m_Load += action;
			}
		}

		internal Loading<T> ObserveAwaiterComplete(Action awaiterComplete)
		{
			if (Disposed) throw new ObjectDisposedException($"{ GetType().Name }:{BundleName}:{AssetName}");
			TryRequest();
			if (IsCompleted)
			{
				awaiterComplete?.Invoke();
			}
			else
			{
				m_AwaiterComplete += awaiterComplete;
			}
			return this;
		}

		public LoadingAwaiter<T> GetAwaiter()
		{
			TryRequest();
			return new LoadingAwaiter<T>(this);
		}

		public void Dispose()
		{
			if (Disposed) return;
			Disposed = true;
			Result = default;
			Error = null;
			m_AwaiterComplete?.Invoke();
			ClearEvent();
		}

		public void TryRequest()
		{
			if (DoRequested) return;
			DoRequested = true;
			RequestImpl();
		}

		protected abstract void RequestImpl();

		protected void OnSuccess(T ret)
		{
			m_Success = true;
			Result = ret;
			m_Load?.Invoke(ret);
			m_AwaiterComplete?.Invoke();
			ClearEvent();
		}

		protected void OnError(Exception ex)
		{
			if (m_RetryCount < MaxRetryCount && RetryHandle != null)
			{
				m_RetryCount++;
				RetryHandle(this, ex, ret =>
				{
					if (ret)
					{
						RequestImpl();
					}
					else
					{
						NofityError(ex);
					}
				});
			}
			else
			{
				NofityError(ex);
			}
		}

		void NofityError(Exception ex)
		{
			if (IgnoreError)
			{
				if (ForceAwaiterCompleteIfError)
				{
					m_AwaiterComplete?.Invoke();
				}
				ClearEvent();
				return;
			}
			Error = ex;
			if (ErrorHandle == null)
			{
				m_AwaiterComplete?.Invoke();
			}
			else
			{
				ErrorHandle.Invoke(ex);
				if (ForceAwaiterCompleteIfError)
				{
					m_AwaiterComplete?.Invoke();
				}
			}
			ClearEvent();
		}

		void ClearEvent()
		{
			m_Load = null;
			m_AwaiterComplete = null;
			RetryHandle = null;
			ErrorHandle = null;
		}

		object IEnumerator.Current => null;

		bool IEnumerator.MoveNext()
		{
			TryRequest();
			return !IsCompleted;
		}

		void IEnumerator.Reset() => throw new NotImplementedException();

		public async Task<T> ToTask()
		{
			return await this;
		}

		public static implicit operator Task<T>(Loading<T> loading)
		{
			return loading.ToTask();
		}

	}

}