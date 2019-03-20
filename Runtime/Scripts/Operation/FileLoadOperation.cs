using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{

	public class FileLoadOperation : LoadOperation
	{

		bool m_abort;
		bool m_error;
		AssetBundleCreateRequest m_loading;

		protected override void Start()
		{
			SetLoadRequst(AssetBundle.LoadFromFileAsync(GetLoadPath()));
		}

		protected void SetLoadRequst(AssetBundleCreateRequest req)
		{
			m_loading = req;
			m_loading.completed += op => OnLoad();
		}

		void OnLoad()
		{
			if (m_abort)
			{
				return;
			}
			var assetBundle = m_loading.assetBundle;
			if (assetBundle != null)
			{
				Success(assetBundle);
			}
			else
			{
				Fail(new System.Exception("load fail."));
			}
			m_loading = null;
		}

		protected override void Abort(System.Action onAbort)
		{
			m_abort = true;
			if (m_loading == null)
			{
				onAbort();
				return;
			}
			var op = m_loading;
			m_loading = null;
			op.completed += (o) =>
			{
				op.assetBundle?.Unload(false);
				onAbort();
			};
		}

		public override void Dispose()
		{

		}
	}

}