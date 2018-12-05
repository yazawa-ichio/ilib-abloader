using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace ILib.AssetBundles
{
	//コルーチン及びUpdateを使わない形のアセットバンドルのローダーです。
	//内部的にコールバック地獄になっているので、バグに対する耐久が低い点と解放漏れをやらかしてリークしやすいのが特徴です。
	//手軽の利用できるという点ではまあまあ？　縛りプレイで作っただけなので、実用にはイマイチ機能が足りていない。
	//個人開発だとこれぐらいで、あとは外でラップしていい感じにフォールバッグやらを付け足したら利用できるかなという程度です。

	/// <summary>
	/// アセットバンドルのローダーです。
	/// 開始時にInitializeで初期化してから利用してください。
	/// </summary>
	public static class ABLoader
	{

		static ABLoaderInstance s_instance;

		/// <summary>
		/// 初期化済みか？
		/// </summary>
		public static bool Initialized => (s_instance != null && s_instance.State == ABLoaderState.Active);

#if UNITY_EDITOR
		private static bool s_useEditorAsset;

		/// <summary>
		/// エディタ上のアセットを直接読み込みます。
		/// 初期化前にフラグをセットしてください。
		/// </summary>
		public static bool UseEditorAsset
		{
			get { return s_useEditorAsset; }
			set
			{
				if (s_instance != null) throw new InvalidOperationException("before Initialize().");
				s_useEditorAsset = value;
			}
		}
#endif

		static int s_maxDownloadCount = 5;
		/// <summary>
		/// 同時ダウンロード数です。
		/// 現在値よりも低い値を設定した場合、適応されるのに遅延があります。
		/// デフォルトは5です。
		/// </summary>
		public static int MaxDownloadCount
		{
			get { return s_maxDownloadCount; }
			set { s_maxDownloadCount = value; s_instance?.SetMaxDownloadCount(value); }
		}

		static int s_maxLoadCount = 10;
		/// <summary>
		/// 同時ロード数です。
		/// 現在値よりも低い値を設定した場合、適応されるのに遅延があります。
		/// デフォルトは10です。
		/// </summary>
		public static int MaxLoadCount
		{
			get { return s_maxLoadCount; }
			set { s_maxLoadCount = value; s_instance?.SeMaxLoadCount(value); }
		}


		/// <summary>
		/// StreamingAssetsのバンドルを利用する場合のオペレーターです。
		/// ディレクトリ名とマニフェストの相対パスを渡してください。
		/// </summary>
		public static InternalLoadOperator CreateInternalOperator(string directory, string manifest)
		{
			var loadPath = System.IO.Path.Combine(Application.streamingAssetsPath, directory);
			return new InternalLoadOperator(loadPath, manifest);
		}

		/// <summary>
		/// サーバからバンドルをダウンロードしてからロードする場合のオペレーターです。
		/// リクエスト時のベースのURLとマニフェストのパスとバージョンを指定してください。
		/// </summary>
		public static NetworkLoadOperator CreateNetworkOperator(string url, string manifest, string version)
		{
			var cache = System.IO.Path.Combine(Application.temporaryCachePath, "AssetBundles");
			return new NetworkLoadOperator(url, cache, manifest, version);
		}

		/// <summary>
		/// 初期化を行います。
		/// ロードの時の処理を決めるオペレーターを指定してください。
		/// オペレーターは手動での作成の他にCreateInternalOperatorやCreateNetworkOperatorで作成ができます。
		/// </summary>
		public static CustomYieldInstruction Initialize(ILoadOperator loadOperator, Action onSuccess, Action<Exception> onFail)
		{
			if (s_instance == null) s_instance = new ABLoaderInstance();
			return s_instance.Initialize(loadOperator, s_maxDownloadCount, s_maxLoadCount, onSuccess, onFail);
		}

		/// <summary>
		/// 現在のリクエストを中断し機能を停止します。
		/// リクエスト元に中断は通知されません。
		/// </summary>
		public static CustomYieldInstruction Stop(Action onComplete = null)
		{
			if (s_instance == null)
			{
				onComplete?.Invoke();
				return new WaitUntil(() => true);
			}
			return s_instance.Stop(() =>
			{
				s_instance = null;
				Cache.Reset();
				onComplete?.Invoke();
			});
		}

		/// <summary>
		/// キャッシュのクリアを行います。
		/// 停止後に実行されるため、実行完了後に再度初期化が必要になります。
		/// </summary>
		public static CustomYieldInstruction CacheClear(Action onComplete = null)
		{
			LogAssert(s_instance != null);
			return s_instance.CacheClear(() =>
			{
				Cache.Reset();
				onComplete?.Invoke();
			});
		}

		/// <summary>
		/// キャッシュファイルが存在するか？
		/// </summary>
		public static bool IsCache(string name)
		{
#if UNITY_EDITOR
			if (UseEditorAsset) return true;
#endif
			return s_instance.IsCache(name);
		}

		/// <summary>
		/// 指定した名前のサイズを返します。
		/// キャッシュ済みかなどは考慮しません。
		/// 標準では機能していません。IBundleDataからサイズを取得できるようにする必要があります。
		/// </summary>
		public static long GetSize(string[] names, bool ignoreDpend = false)
		{
#if UNITY_EDITOR
			if (UseEditorAsset) return 0;
#endif
			return s_instance.GetSize(names, ignoreDpend);
		}

		/// <summary>
		/// 指定した名前のダウンロードが必要なサイズを返します。
		/// 標準では機能していません。IBundleDataからサイズを取得できるようにする必要があります。
		/// </summary>
		public static long GetDownloadSize(string[] names)
		{
#if UNITY_EDITOR
			if (UseEditorAsset) return 0;
#endif
			names = s_instance.GetDownloadList(names);
			return s_instance.GetSize(names);
		}

		/// <summary>
		/// ファイルの一括ダウンロードを行います。
		/// すべての処理が終了した際にonCompleteが実行されます。
		/// 失敗したリクエストがあった場合はfalseが引数に渡されます。
		/// onFailは失敗したすべてのリクエストに対して実行されます。
		/// 進捗を取得したい場合は返り値を利用してください。
		/// </summary>
		public static Func<float> Download(string[] names, Action<bool> onComplete, Action<Exception> onFail = null)
		{
			if (!Initialized)
			{
				onFail?.Invoke(new InvalidOperationException());
				onComplete?.Invoke(false);
				return () => 0;
			}
#if UNITY_EDITOR
			if (UseEditorAsset)
			{
				onComplete?.Invoke(true);
				return () => 1;
			}
#endif
			return s_instance.Download(names, onComplete, onFail);
		}

		/// <summary>
		/// アセットバンドルをロードします。
		/// BundleContainerRefからアセットをロードし、不要になった際にDisposeを実行してください。
		/// Disposeを実行し忘れるとバンドルがリークします。逆にDiposeするまでバンドルはキャッシュされます。
		/// </summary>
		public static void LoadContainer(string name, Action<BundleContainerRef> onSuccess, Action<Exception> onFail)
		{

			if (!Initialized)
			{
				onFail?.Invoke(new InvalidOperationException());
				return;
			}

#if UNITY_EDITOR
			if (UseEditorAsset)
			{
				try
				{
					onSuccess?.Invoke(new BundleContainerRef(new EditorContainer(name)));
				}
				catch (Exception ex)
				{
					LogError(ex);
				}
				return;
			}
#endif

			s_instance.LoadContainer(name, onSuccess, onFail);
		}

		/// <summary>
		/// アセットバンドルからアセットを同期ロードします。
		/// コンテナへの参照は自動で解除します。
		/// </summary>
		public static void LoadAsset<T>(string name, string assetName, Action<T> onSuccess, Action<Exception> onFail) where T : UnityEngine.Object
		{
			LoadContainer(name, (contaner) =>
			{
				var asset = contaner.LoadAsset<T>(assetName);
				contaner.Dispose();
				onSuccess?.Invoke(asset);
			}, onFail);
		}

		/// <summary>
		/// アセットバンドルからアセットを非同期ロードします。
		/// コンテナへの参照は自動で解除します。
		/// </summary>
		public static void LoadAssetAsync<T>(string name, string assetName, Action<T> onSuccess, Action<Exception> onFail) where T : UnityEngine.Object
		{
			LoadContainer(name, (contaner) =>
			{
				contaner.LoadAssetAsync<T>(assetName, asset =>
				{
					contaner.Dispose();
					onSuccess?.Invoke(asset);
				});
			}, onFail);
		}

		/// <summary>
		/// アセットバンドルからシーンを同期ロードします。
		/// コンテナへの参照は自動で解除します。
		/// </summary>
		public static void LoadScene(string name, string scaneName, UnityEngine.SceneManagement.LoadSceneMode mode, Action onSuccess, Action<Exception> onFail)
		{
			LoadContainer(name, (contaner) =>
			{
				contaner.LoadScene(scaneName, mode);
				contaner.Dispose();
				onSuccess?.Invoke();
			}, onFail);
		}

		/// <summary>
		/// アセットバンドルからシーンを非同期ロードします。
		/// コンテナへの参照は自動で解除します。
		/// </summary>
		public static void LoadSceneAsync(string name, string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode, Action onSuccess, Action<Exception> onFail)
		{
			LoadContainer(name, (contaner) =>
			{
				contaner.LoadSceneAsync(sceneName, mode, () =>
				{
					contaner.Dispose();
					onSuccess?.Invoke();
				});
			}, onFail);
		}

		static Action<Exception> s_onLogError;
		static Action<string> s_onLogAssert;

		/// <summary>
		/// 例外をハンドリングした際に吐き出すログを出力を指定します。
		/// 標準のログでそのまま出力されたくない場合に利用してください。
		/// </summary>
		public static void HandleErrorLog(Action<Exception> onError)
		{
			s_onLogError = onError;
		}

		/// <summary>
		///	Assertの出力を指定します。
		/// 標準のログでそのまま出力されたくない場合に利用してください。
		/// </summary>
		public static void HandleAssert(Action<string> onAssert)
		{
			s_onLogAssert = onAssert;
		}

		internal static void LogError(Exception ex)
		{
			if (s_onLogError != null)
			{
				s_onLogError(ex);
			}
			else
			{
				Debug.LogError(ex);
			}
		}

		internal static void LogAssert(bool condition)
		{
			if (s_onLogAssert == null)
			{
				Debug.Assert(condition);
			}
		}

		internal static void LogAssert(bool condition, string message)
		{
			if (s_onLogAssert == null)
			{
				Debug.Assert(condition, message);
			}
			else if (condition)
			{
				s_onLogAssert(message);
			}
		}


	}


}