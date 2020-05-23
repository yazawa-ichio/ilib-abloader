namespace ILib.AssetBundles
{
	public class AssetLoading<T> : Loading<T> where T : UnityEngine.Object
	{
		internal AssetLoading(string bundleName, string assetName) : base(bundleName, assetName)
		{
		}

		protected override void RequestImpl()
		{
			ABLoader.LoadAssetAsync<T>(BundleName, AssetName, x => OnSuccess(x), OnError);
		}
	}

}