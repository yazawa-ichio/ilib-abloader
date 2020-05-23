using System;

namespace ILib.AssetBundles
{
	public class LoadingConfig
	{
		public int MaxRetryCount = 5;
		public bool IgnoreError;
		public Action<ILoading, Exception, Action<bool>> RetryHandle;
		public Action<Exception> ErrorHandle;
	}

}