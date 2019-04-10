using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ILib.AssetBundles
{
	internal static class Cache
	{
		static ILoadOperator s_LoadOperator;
		public static void Init(ILoadOperator loadOperator)
		{
			s_LoadOperator = loadOperator;
		}

		public static void Reset()
		{
			s_LoadOperator = null;
		}

		public static string GetLoadPath(string name, string hash)
		{
			return s_LoadOperator.LoadPath(name, hash);
		}

		public static void Delete(string name)
		{
			var path = s_LoadOperator.LoadPath(name, "");
			var dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
			{
				return;
			}
			var files = Directory.GetFiles(dir, Path.GetFileName(name) + "*");
			foreach (var file in files)
			{
				File.Delete(file);
			}
		}

		public static System.Exception TryDelete(string name)
		{
			try
			{
				Delete(name);
				return null;
			}
			catch (System.Exception ex)
			{
				return ex;
			}
		}

		public static System.Exception TryDelete(string name, string hash)
		{
			try
			{
				if (!s_LoadOperator.IsDownload(name, hash)) return null;
				Delete(name);
				return null;
			}
			catch (System.Exception ex)
			{
				return ex;
			}
		}


		public static void DeleteAll()
		{
			var path = s_LoadOperator.GetCacheRoot();
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}


	}

}
