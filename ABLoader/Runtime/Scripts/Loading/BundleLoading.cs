using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ILib.AssetBundles
{
	public class BundleLoading : Loading<BundleContainerRef>
	{
		internal BundleLoading(string bundleName) : base(bundleName, null)
		{
		}

		protected override void RequestImpl()
		{
			ABLoader.LoadContainer(BundleName, x => OnSuccess(x), OnError);
		}
	}
}