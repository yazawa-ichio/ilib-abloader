using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	using Logger;

	public abstract class LoadOperation : IRequest
	{
		ABLoaderInstance m_Owner;
		internal System.Action<BundleRef> OnSuccess;
		internal System.Action<System.Exception> OnFail;
		IRequestHander m_Hander;
		ILoadOperator m_LoadOperator;

		public string Name { get; private set; }
		public string Hash { get; private set; }
		public uint CRC { get; private set; }
		public bool IsRunning { get; private set; }

		internal void Init(ILoadOperator loadOperator,string name, string hash, uint crc, ABLoaderInstance owner)
		{
			m_LoadOperator = loadOperator;
			Name = name;
			Hash = hash;
			CRC = crc;
			m_Owner = owner;
		}

		public virtual void Reset()
		{
			m_Owner = null;
			OnSuccess = null;
			OnFail = null;
			m_Hander = null;
			Name = null;
			Hash = null;
			CRC = 0;
			IsRunning = false;
		}

		void IRequest.SetHander(IRequestHander hander)
		{
			m_Hander = hander;
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
			Log.Trace("[ilib-abloader] load success {0}.", Name);
			IsRunning = false;
			m_Hander.OnComplete(this);
			var bundleRef = m_Owner.CreateBundleRef(Name, bundle);
			bundleRef.AddRef();
			OnSuccess(bundleRef);
			bundleRef.RemoveRef();
			m_LoadOperator?.CompleteLoad(this);
		}

		internal protected void Fail(System.Exception ex)
		{
			Log.Warning("[ilib-abloader] load fail {0}. {1}", Name, ex);
			IsRunning = false;
			Cache.TryDelete(Name, Hash);
			m_Hander.OnComplete(this);
			OnFail(ex);
			m_LoadOperator?.CompleteLoad(this);
		}

	}
}
