using System;
using System.Runtime.CompilerServices;

namespace ILib.AssetBundles
{
	public class ContainerSceneLoading : ICriticalNotifyCompletion
	{
		public bool IsCompleted { get; private set; }

		Action m_Continuation;

		internal void SetResult()
		{
			IsCompleted = true;
			m_Continuation?.Invoke();
		}

		public void OnCompleted(Action continuation)
		{
			if (IsCompleted)
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

		public void GetResult()
		{
		}

		public ContainerSceneLoading GetAwaiter() => this;

	}
}