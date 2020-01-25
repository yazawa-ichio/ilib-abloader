using UnityEngine.SceneManagement;

namespace ILib.AssetBundles
{
	public class SceneLoading : Loading<bool>
	{
		public LoadSceneMode Mode { get; private set; } = LoadSceneMode.Additive;

		internal SceneLoading(string bundleName, string sceneName) : base(bundleName, sceneName)
		{
		}

		protected override void RequestImpl()
		{
			ABLoader.LoadSceneAsync(BundleName, AssetName, Mode, () => OnSuccess(true), OnError);
		}
	}

}