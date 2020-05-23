using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ILib.AssetBundles
{
	using Logger;

	internal static class Cache
	{
		static ILoadOperator s_LoadOperator;
		static HashSet<string> s_Exsits;
		public static void Init(ILoadOperator loadOperator)
		{
			s_LoadOperator = loadOperator;
			s_Exsits = new HashSet<string>();
		}

		public static void Reset()
		{
			Log.Debug("[ilib-abloader] cache reset.");
			s_LoadOperator = null;
			s_Exsits.Clear();
		}

		public static bool IsExists(string name, string hash)
		{
			var path = GetLoadPath(name, hash);
			if (s_Exsits.Contains(path)) return true;
			var ret = File.Exists(path);
			if (ret) s_Exsits.Add(path);
			Log.Trace("[ilib-abloader] cache exists {0}", path);
			return ret;
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
				s_Exsits.Remove(file);
				Log.Trace("[ilib-abloader] cache delete {0}", file);
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
				Log.Error("[ilib-abloader] cache try delete fail. {0}", ex);
				return ex;
			}
		}


		public static void DeleteAll()
		{
			Log.Debug("[ilib-abloader] cache delete all.");
			s_Exsits.Clear();
			var path = s_LoadOperator.GetCacheRoot();
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}


	}

}