﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{

	public abstract class Initializer
	{
		IBundleData m_data;
		Action<IBundleData, Exception> m_onComplete;

		public void AddCompleteEvent(Action<IBundleData, Exception> onComplete)
		{
			m_onComplete += onComplete;
		}

		internal void DoStart()
		{
			Start();
		}

		protected abstract void Start();

		protected void Success(IBundleData data)
		{
			m_onComplete(data, null);
		}

		protected void Fail(Exception ex)
		{
			m_onComplete(null, ex);
		}

		protected virtual void Load(string loadPath, string assetName)
		{
			var bundle = AssetBundle.LoadFromFile(loadPath);
			if (bundle == null)
			{
				Fail(new System.Exception("load fail bundle."));
				return;
			}
			var asset = bundle.LoadAsset(assetName);
			if (asset != null && asset is IBundleDataProvider)
			{
				bundle.Unload(false);
				Success((asset as IBundleDataProvider).Provide());
			}
			else if (asset is IBundleData)
			{
				bundle.Unload(false);
				Success(asset as IBundleData);
			}
			else
			{
				var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
				if (manifest == null)
				{
					Fail(new System.Exception("load fail manifest."));
					return;
				}
				bundle.Unload(false);
				Success(new BundleData(manifest));
			}
		}

		protected void Download(string url, string name, string cachePath, Action onSuccess)
		{
			var request = new DownloadRequest
			{
				Name = name,
				Url = url,
				CachePath = cachePath,
				OnSuccess = onSuccess,
				OnFail = Fail,
			};
			request.DoStart();
		}

	}

}