using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ILib.AssetBundles
{
	public readonly struct LoadingAwaiter<T> : ICriticalNotifyCompletion
	{
		readonly Loading<T> m_Loading;

		public bool IsCompleted => m_Loading.IsCompleted;

		public LoadingAwaiter(Loading<T> loading)
		{
			m_Loading = loading;
		}

		public void OnCompleted(Action continuation)
		{
			if (m_Loading.IsCompleted)
			{
				continuation();
				return;
			}
			m_Loading.ObserveAwaiterComplete(continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			OnCompleted(continuation);
		}

		public T GetResult()
		{
			if (m_Loading.Disposed)
			{
				throw new TaskCanceledException("loading is canceled.");
			}
			if (m_Loading.Error != null)
			{
				throw m_Loading.Error;
			}
			return m_Loading.Result;
		}

	}

}