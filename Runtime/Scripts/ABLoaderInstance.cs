using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ILib.AssetBundles
{
	/// <summary>
	/// ABLoaderの実体です
	/// </summary>
	internal class ABLoaderInstance
	{

		public ABLoaderState State { get; private set; } = ABLoaderState.None;

		IBundleData m_BundleData;
		ILoadOperator m_LoadOperator;
		RequestHander<DownloadRequest> m_Downloader;
		RequestHander<LoadOperation> m_Loader;
		Dictionary<string, BundleContainer> m_Container = new Dictionary<string, BundleContainer>();
		Dictionary<string, BundleRef> m_LoadedBundles = new Dictionary<string, BundleRef>();
		List<BundleRef> m_UnloadList = new List<BundleRef>();


		public void SetMaxDownloadCount(int count)
		{
			m_Downloader.MaxCount = count;
		}

		public void SeMaxLoadCount(int count)
		{
			m_Loader.MaxCount = count;
		}

		public CustomYieldInstruction Initialize(ILoadOperator loadOperator, int maxDownloadCount, int maxLoadCount, Action onSuccess, Action<Exception> onFail)
		{
			bool complete = false;
			var wait = new WaitUntil(() => complete);

			//起動済みの際はリセットを先にする必要がある
			if (State != ABLoaderState.None)
			{
				complete = true;
				onFail?.Invoke(new InvalidOperationException("use ABLoader.Stop()"));
				return wait;
			}

			State = ABLoaderState.Initialize;
			Cache.Init(loadOperator);
			m_LoadOperator = loadOperator;
			m_Downloader = new RequestHander<DownloadRequest>(maxDownloadCount);
			m_Loader = new RequestHander<LoadOperation>(maxLoadCount);

#if UNITY_EDITOR
			if (ABLoader.UseEditorAsset)
			{
				State = ABLoaderState.Active;
				onSuccess?.Invoke();
				complete = true;
				return wait;
			}
#endif
			var initializer = m_LoadOperator.Init();
			initializer.AddCompleteEvent((data, ex) =>
			{
				complete = true;
				if (ex != null)
				{
					State = ABLoaderState.Error;
					onFail(ex);
				}
				else if (data != null)
				{
					State = ABLoaderState.Active;
					m_BundleData = data;
					onSuccess?.Invoke();
				}
			});
			initializer.DoStart();
			return wait;
		}

		public CustomYieldInstruction Stop(Action onComplete)
		{
			if (State == ABLoaderState.Abort) new InvalidOperationException("current aborting.");
			bool complete = false;
			State = ABLoaderState.Abort;
			//ダウンロードの中断は即実行される
			m_Downloader.Abort(() =>
			{
				m_Loader.Abort(() =>
				{
					lock (m_UnloadList)
					{
						m_UnloadList.Clear();
					}
					foreach (var bundleRef in m_LoadedBundles.Values)
					{
						bundleRef.Dispose();
					}
					m_LoadedBundles.Clear();
					foreach (var container in m_Container.Values.ToArray())
					{
						container.Dispose();
					}
					m_Container.Clear();
					State = ABLoaderState.Stop;
					complete = true;
					onComplete?.Invoke();
				});
			});
			return new WaitUntil(() => complete);
		}

		public CustomYieldInstruction CacheClear(Action onComplete)
		{
			return Stop(() =>
			{
				Cache.DeleteAll();
				onComplete?.Invoke();
			});
		}

		public bool IsCache(string name)
		{
			if (m_Downloader.HasRequest(name)) return false;
			return System.IO.File.Exists(Cache.GetLoadPath(name, m_BundleData.GetHash(name)));
		}

		public bool IsCache(string name, string hash)
		{
			if (m_Downloader.HasRequest(name)) return false;
			return System.IO.File.Exists(Cache.GetLoadPath(name, hash));
		}

		public IEnumerable<string> GetIncludedDependList(string[] names)
		{
			HashSet<string> list = new HashSet<string>();
			for (int i = 0; i < names.Length; i++)
			{
				var name = names[i];
				list.Add(name);
				list.UnionWith(m_BundleData.GetAllDepends(name));
			}
			return list;
		}

		public string[] GetDownloadList(string[] names)
		{
			return GetIncludedDependList(names).Where(x =>
			{
				string hash = m_BundleData.GetHash(x);
				return m_LoadOperator.IsDownload(x, hash) && !IsCache(x, hash);
			}).ToArray();
		}

		public long GetSize(string[] names, bool ignoreDpend = false)
		{
			if (!ignoreDpend)
			{
				names = GetIncludedDependList(names).ToArray();
			}
			long size = 0;
			for (int i = 0; i < names.Length; i++)
			{
				size += m_BundleData.GetSize(names[i]);
			}
			return size;
		}

		public Func<float> Download(string[] names, Action<bool> onComplete, Action<Exception> onFail = null)
		{

			names = GetDownloadList(names);

			//すでに依存関係を考慮している
			long size = GetSize(names, ignoreDpend: true);
			int successCount = 0;
			int completeCount = 0;

			DownloadRequest[] requests = new DownloadRequest[names.Length];
			Func<float> onProgress = () =>
			{
				int count = requests.Length;
				double sum = 0;
				for (int i = 0; i < count; i++)
				{
					var req = requests[i];
					if (req != null) sum += req.GetProgress() * m_BundleData.GetSize(req.Name);
				}
				return (float)(sum / size);
			};

			Action onSuccessOnce = () =>
			{
				completeCount++;
				successCount++;
				if (names.Length == completeCount)
				{
					onComplete?.Invoke(completeCount == successCount);
				}
			};

			Action<Exception> onFailOnce = (ex) =>
			{
				completeCount++;
				if (names.Length == completeCount)
				{
					onComplete?.Invoke(false);
				}
				onFail?.Invoke(ex);
			};

			for (int i = 0; i < names.Length; i++)
			{
				var name = names[i];
				var hash = m_BundleData.GetHash(name);
				var url = m_LoadOperator.RequestUrl(name, hash);
				requests[i] = DownloadRequest(url, name, hash, onSuccessOnce, onFailOnce);
			}

			return onProgress;
		}

		public void LoadContainer(string name, Action<BundleContainerRef> onSuccess, Action<Exception> onFail)
		{

			BundleContainer container;
			if (m_Container.TryGetValue(name, out container))
			{
				container.SetEvent(onSuccess, onFail);
				return;
			}

			var deps = m_BundleData.GetAllDepends(name);
			container = new BundleContainer(name, deps.Length, this);
			container.SetEvent(onSuccess, onFail);

			Action<BundleRef> onLoad = container.OnLoad;
			Action<Exception> onLoadFail = container.OnFail;

			Request(name, m_BundleData.GetHash(name), onLoad, onLoadFail);
			for (int i = 0; i < deps.Length; i++)
			{
				Request(deps[i], m_BundleData.GetHash(deps[i]), onLoad, onLoadFail);
			}
		}

		void Request(string name, string hash, Action<BundleRef> onLoad, Action<Exception> onFail)
		{
			//ロード済だったらすぐに返す
			BundleRef bundleRef = null;
			if (m_LoadedBundles.TryGetValue(name, out bundleRef))
			{
				ABLoader.LogAssert(bundleRef != null);
				if (bundleRef.Bundle != null)
				{
					onLoad(bundleRef);
					return;
				}
				else
				{
					//管理していない形でアンロードされた
					m_LoadedBundles.Remove(name);
				}
			}

			if (!m_LoadOperator.IsDownload(name, hash) || IsCache(name, hash))
			{
				LoadRequest(name, hash, onLoad, onFail);
			}
			else
			{
				var url = m_LoadOperator.RequestUrl(name, hash);
				DownloadRequest(url, name, hash, () => LoadRequest(name, hash, onLoad, onFail), onFail);
			}
		}

		void LoadRequest(string name, string hash, Action<BundleRef> onLoad, Action<Exception> onFail)
		{
			//リクエスト済みなら連結
			LoadOperation op;
			if (m_Loader.TryGetRequset(name, out op))
			{
				op.OnSuccess += onLoad;
				op.OnFail += onFail;
				return;
			}

			op = m_LoadOperator.Load(name, hash);
			op.Init(name, hash, this);
			op.OnSuccess += onLoad;
			op.OnFail += onFail;
			m_Loader.Request(op);

		}

		DownloadRequest DownloadRequest(string url, string name, string hash, Action onSuccess, Action<Exception> onFail)
		{
			DownloadRequest req = null;
			if (m_Downloader.TryGetRequset(name, out req))
			{
				req.OnSuccess += onSuccess;
				req.OnFail += onFail;
				return req;
			}

			//古いキャッシュを削除
			var ex = Cache.TryDelete(name);
			if (ex != null)
			{
				onFail?.Invoke(ex);
				return req;
			}

			//リクエストを作成
			req = new DownloadRequest
			{
				Name = name,
				Url = url,
				CachePath = Cache.GetLoadPath(name, hash),
				OnSuccess = onSuccess,
				OnFail = onFail,
			};

			m_Downloader.Request(req);

			return req;
		}

		internal void Unload()
		{
			lock (m_UnloadList)
			{
				foreach (var bundleRef in m_UnloadList)
				{
					if (!bundleRef.HasRef)
					{
						m_LoadedBundles.Remove(bundleRef.Name);
						bundleRef.Dispose();
					}
				}
				m_UnloadList.Clear();
			}
		}

		internal BundleRef CreateBundleRef(string name, AssetBundle bundle)
		{
			return m_LoadedBundles[name] = new BundleRef(this, name, bundle);
		}

		internal void UnloadRef(BundleRef bundleRef)
		{
			switch (ABLoader.UnloadMode)
			{
				case UnloadMode.Immediately:
					m_LoadedBundles.Remove(bundleRef.Name);
					bundleRef.Dispose();
					break;
				case UnloadMode.Manual:
				case UnloadMode.Auto:
					lock (m_UnloadList)
					{
						m_UnloadList.Add(bundleRef);
					}
					break;
			}
		}

		internal void UnloadContainer(BundleContainer container)
		{
			m_Container.Remove(container.Name);
		}

	}

}
