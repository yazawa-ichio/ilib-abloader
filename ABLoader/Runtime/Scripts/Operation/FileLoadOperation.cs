using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{

	public class FileLoadOperation : LoadOperation
	{

		bool m_Abort;
		AssetBundleCreateRequest m_Loading;

		protected override void Start()
		{
			SetLoadRequst(AssetBundle.LoadFromFileAsync(GetLoadPath()));
		}

		protected void SetLoadRequst(AssetBundleCreateRequest req)
		{
			m_Loading = req;
			m_Loading.completed += op => OnLoad();
		}

		void OnLoad()
		{
			if (m_Abort)
			{
				return;
			}
			var assetBundle = m_Loading.assetBundle;
			if (assetBundle != null)
			{
				Success(assetBundle);
			}
			else
			{
				Fail(new FileLoadException($"{Name} load fail. path:{GetLoadPath()}", Name, GetLoadPath()));
			}
			m_Loading = null;
		}

		protected override void Abort(System.Action onAbort)
		{
			m_Abort = true;
			if (m_Loading == null)
			{
				onAbort();
				return;
			}
			var op = m_Loading;
			m_Loading = null;
			op.completed += (o) =>
			{
				op.assetBundle?.Unload(false);
				onAbort();
			};
		}

		public override void Reset()
		{
			base.Reset();
			m_Abort = false;
			m_Loading = null;
		}

		public override void Dispose()
		{
		}
	}

}
