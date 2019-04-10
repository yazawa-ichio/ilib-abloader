using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILib.AssetBundles.NameSetter;
using System.IO;
using System.Linq;

namespace ILib.AssetBundles.ExManifest.Tools
{

	public class ExDataProvider : IExDataProvider
	{
		List<ITagCollector> m_TagCollectors = new List<ITagCollector>();
		List<IReferenceCollector> m_RefCollectors = new List<IReferenceCollector>();

		public ExDataProvider()
		{
			foreach (var ctx in SetterAssetCache.GetAllContexts())
			{
				if (ctx is ITagCollector)
				{
					m_TagCollectors.Add(ctx as ITagCollector);
				}
				if (ctx is IReferenceCollector)
				{
					m_RefCollectors.Add(ctx as IReferenceCollector);
				}
			}
		}

		public string GetReferenceId(string bundleName, string path)
		{
			foreach (var ctx in m_RefCollectors)
			{
				var id = ctx.GetId(bundleName, path);
				if (!string.IsNullOrEmpty(id))
				{
					return id;
				}
			}
			return "";
		}

		public string[] GetTag(string bundleName, string[] paths)
		{
			return GetTagImpl(bundleName, paths).Distinct().ToArray();
		}

		IEnumerable<string> GetTagImpl(string bundleName, string[] paths)
		{
			foreach (var ctx in m_TagCollectors)
			{
				foreach (var tag in ctx.GetTags(bundleName, paths))
				{
					yield return tag;
				}
			}
		}

	}

}
