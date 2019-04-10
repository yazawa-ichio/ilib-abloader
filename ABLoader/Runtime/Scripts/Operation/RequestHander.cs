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
		Dictionary<string, T> m_Requests = new Dictionary<string, T>();
		Queue<T> m_RequestQueue = new Queue<T>();
		int m_ProcessingCount = 0;
		int m_MaxCount;
		public int MaxCount
		{
			get { return m_MaxCount; }
			set
			{
				m_MaxCount = value;
				TryNextRequest();
			}
		}

		public RequestHander(int maxCount)
		{
			m_MaxCount = maxCount;
		}

		public bool HasRequest(string name)
		{
			return m_Requests.ContainsKey(name);
		}

		public bool TryGetRequset(string name, out T request)
		{
			return m_Requests.TryGetValue(name, out request);
		}

		public void Request(T request)
		{
			request.SetHander(this);
			m_Requests[request.Name] = request;
			m_RequestQueue.Enqueue(request);
			TryNextRequest();
		}

		public void OnComplete(IRequest request)
		{
			m_ProcessingCount--;
			m_Requests.Remove(request.Name);
			TryNextRequest();
			request.Dispose();
		}

		void TryNextRequest()
		{
			while (m_RequestQueue.Count > 0 && m_ProcessingCount < m_MaxCount)
			{
				var req = m_RequestQueue.Dequeue();
				req.DoStart();
				m_ProcessingCount++;
			}
		}

		public void Abort(Action onAbort)
		{
			m_RequestQueue.Clear();
			var runningRequests = m_Requests.Values.Where(op => op.IsRunning).ToArray();
			m_Requests.Clear();
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
