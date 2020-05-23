using System;
using System.Runtime.CompilerServices;

namespace ILib.AssetBundles
{
	public class ContainerAssetLoading<T> : ICriticalNotifyCompletion where T : UnityEngine.Object
	{
		public bool IsCompleted => m_Result != null;

		T m_Result;
		Action m_Continuation;

		internal void SetResult(T ret)
		{
			m_Result = ret;
			m_Continuation?.Invoke();
		}

		public void OnCompleted(Action continuation)
		{
			if (m_Result != null)
			{
				continuation?.Invoke();
			}
			else
			{
				m_Continuation += continuation;
			}
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			OnCompleted(continuation);
		}

		public T GetResult()
		{
			return m_Result;
		}

		public ContainerAssetLoading<T> GetAwaiter() => this;

	}
}