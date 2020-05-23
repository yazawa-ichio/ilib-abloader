using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ILib.AssetBundles.NameSetter
{
	public static class SetterAssetCache
	{
		static Dictionary<string, SetterAsset> s_Dic = new Dictionary<string, SetterAsset>();

		public static void SetDirty()
		{
			s_Dic.Clear();
		}

		public static IEnumerable<T> GetRootSetters<T>(string path)
		{
			return GetRootSetters(path).Where(x => x is T).Cast<T>();
		}

		public static IEnumerable<SetterContext> GetRootSetters(string path)
		{
			HashSet<System.Type> types = new HashSet<System.Type>();
			foreach (var setter in GetSetterAssets(path))
			{
				foreach (var ctx in setter.m_Contexts)
				{
					if (types.Add(ctx.GetType()))
					{
						yield return ctx;
					}
				}
			}
		}

		public static IEnumerable<T> GetSetters<T>(string path) where T : SetterContext
		{
			foreach (var setter in GetSetterAssets(path))
			{
				foreach (var ctx in setter.m_Contexts)
				{
					if (ctx.GetType() == typeof(T))
					{
						yield return ctx as T;
					}
				}
			}
		}

		public static IEnumerable<SetterAsset> GetSetterAssets(string path)
		{
			return GetSetterAssetsImpl(path).Reverse();
		}

		static IEnumerable<SetterAsset> GetSetterAssetsImpl(string path)
		{
			var dirPath = path.Substring(0, path.LastIndexOf(Path.GetFileName(path)) - 1);
			while (true)
			{
				foreach (var filePath in Directory.GetFiles(dirPath, "*." + SetterAssetImporter.Ext, SearchOption.TopDirectoryOnly))
				{
					string fileName = Path.GetFileName(filePath);
					yield return GetAsset(dirPath + "/" + fileName);
				}
				var index = dirPath.LastIndexOf(Path.GetFileName(dirPath));
				if (index <= 0)
				{
					break;
				}
				dirPath = dirPath.Substring(0, index - 1);
			}
		}

		static SetterAsset GetAsset(string path)
		{
			SetterAsset asset;
			if (!s_Dic.TryGetValue(path, out asset))
			{
				s_Dic[path] = asset = Serializer.Deserialize(path);
			}
			return asset;
		}

		public static IEnumerable<SetterContext> GetAllContexts()
		{
			foreach (var path in Directory.GetFiles("Assets/", "*." + SetterAssetImporter.Ext, SearchOption.AllDirectories))
			{
				var asset = GetAsset(path);
				if (asset != null)
				{
					foreach (var ctx in asset.m_Contexts)
					{
						yield return ctx;
					}
				}
			}
		}

	}
}