using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	public abstract class LoadOperation : IRequest
	{
		ABLoaderInstance m_owner;
		internal System.Action<BundleRef> onSuccess;
		internal System.Action<System.Exception> onFail;
		IRequestHander m_hander;

		public string Name { get; private set; }
		public string Hash { get; private set; }
		public bool IsRunning { get; private set; }

		internal void Init(string name, string hash, ABLoaderInstance owner)
		{
			this.Name = name;
			this.Hash = hash;
			m_owner = owner;
		}

		void IRequest.SetHander(IRequestHander hander)
		{
			m_hander = hander;
		}

		void IRequest.DoStart()
		{
			IsRunning = true;
			Start();
		}

		void IRequest.DoAbort(System.Action onAbort)
		{
			Abort(onAbort);
		}

		protected abstract void Start();
		protected abstract void Abort(System.Action onAbort);
		public abstract void Dispose();

		protected string GetLoadPath()
		{
			return Cache.GetLoadPath(Name, Hash);
		}

		protected void Success(AssetBundle bundle)
		{
			IsRunning = false;
			m_hander.OnComplete(this);
			var bundleRef = m_owner.CreateBundleRef(Name, bundle);
			bundleRef.AddRef();
			onSuccess(bundleRef);
			bundleRef.RemoveRef();
		}

		internal protected void Fail(System.Exception ex)
		{
			IsRunning = false;
			Cache.TryDelete(Name, Hash);
			m_hander.OnComplete(this);
			onFail(ex);
		}

	}
}