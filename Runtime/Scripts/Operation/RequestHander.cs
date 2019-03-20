using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ILib.AssetBundles
{

	internal interface IRequest : System.IDisposable
	{
		bool IsRunning { get; }
		string Name { get; }
		void SetHander(IRequestHander hander);
		void DoStart();
		void DoAbort(Action onComplete);
	}

	internal interface IRequestHander
	{
		void OnComplete(IRequest request);
	}

	internal class RequestHander<T> : IRequestHander where T : IRequest
	{
		Dictionary<string, T> m_requests = new Dictionary<string, T>();
		Queue<T> m_requestQueue = new Queue<T>();
		int m_processingCount = 0;
		int m_maxCount;
		public int MaxCount
		{
			get { return m_maxCount; }
			set
			{
				m_maxCount = value;
				TryNextRequest();
			}
		}

		public RequestHander(int maxCount)
		{
			m_maxCount = maxCount;
		}

		public bool HasRequest(string name)
		{
			return m_requests.ContainsKey(name);
		}

		public bool TryGetRequset(string name, out T request)
		{
			return m_requests.TryGetValue(name, out request);
		}

		public void Request(T request)
		{
			request.SetHander(this);
			m_requests[request.Name] = request;
			m_requestQueue.Enqueue(request);
			TryNextRequest();
		}

		public void OnComplete(IRequest request)
		{
			m_processingCount--;
			m_requests.Remove(request.Name);
			TryNextRequest();
			request.Dispose();
		}

		void TryNextRequest()
		{
			while (m_requestQueue.Count > 0 && m_processingCount < m_maxCount)
			{
				var req = m_requestQueue.Dequeue();
				req.DoStart();
				m_processingCount++;
			}
		}

		public void Abort(Action onAbort)
		{
			m_requestQueue.Clear();
			var runningRequests = m_requests.Values.Where(op => op.IsRunning).ToArray();
			m_requests.Clear();
			if (runningRequests.Length == 0)
			{
				onAbort?.Invoke();
				return;
			}
			var count = runningRequests.Length;
			Action onAbortOnce = () =>
			{
				count--;
				if (count == 0) onAbort();
			};
			for (int i = 0; i < runningRequests.Length; i++)
			{
				runningRequests[i].DoAbort(onAbortOnce);
			}
		}

	}

}