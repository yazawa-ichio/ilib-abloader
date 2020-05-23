using System;
using UnityEngine.Networking;

namespace ILib.AssetBundles
{
	public class FileLoadException : Exception
	{
		public string Name { get; private set; }

		public string Path { get; private set; }

		public FileLoadException(string message, string name, string path) : base(message)
		{
			Name = name;
			Path = path;
		}

	}

	public class DownloadException : Exception
	{
		public UnityWebRequest Request { get; private set; }

		public DownloadException(string message, UnityWebRequest request) : base(message)
		{
			Request = request;
		}
	}

}