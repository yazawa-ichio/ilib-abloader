using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

namespace ILib.AssetBundles.NameSetter
{

	public static class Util
	{
		public static readonly Type[] ContextTypes;

		static Util()
		{
			ContextTypes = GetContexts().ToArray();
		}

		static IEnumerable<Type> GetContexts()
		{
			foreach (var assemblie in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assemblie.GetTypes())
				{
					if (type.IsAbstract)
					{
						continue;
					}
					var baseType = type.BaseType;
					while (baseType != null)
					{
						if (baseType == typeof(SetterContext))
						{
							yield return type;
							break;
						}
						baseType = baseType.BaseType;
					}
				}
			}
		}

		public static bool IsTargetExt(string path, string targetExt)
		{
			var ext = Path.GetExtension(path).Substring(1);
			if (ext == SetterAssetImporter.Ext)
			{
				return false;
			}
			if (targetExt == "*") return true;
			foreach (var target in targetExt.Split(','))
			{
				if (string.Compare(ext, target, true) == 0)
				{
					return true;
				}
			}
			return false;
		}
	}

}
