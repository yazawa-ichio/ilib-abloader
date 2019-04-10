using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Linq;
using System.Text;

namespace ILib.AssetBundles.NameSetter
{

	public static class Serializer
	{
		static XmlSerializer s_XmlSerializer;

		static Serializer()
		{
			s_XmlSerializer = new XmlSerializer(typeof(List<SetterContext>), Util.ContextTypes);
		}


		public static string Serialize(SetterAsset asset)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				s_XmlSerializer.Serialize(ms, asset.m_Contexts);
				ms.Seek(0, SeekOrigin.Begin);
				return new StreamReader(ms).ReadToEnd();
			}
		}

		public static void Serialize(SetterAsset asset, string path)
		{
			using (var fs = new FileStream(path, FileMode.Create))
			{
				s_XmlSerializer.Serialize(fs, asset.m_Contexts);
				EditorUtility.SetDirty(asset);
			}
		}

		public static SetterAsset Deserialize(string path)
		{
			SetterAsset asset = ScriptableObject.CreateInstance<SetterAsset>();
			using (var fs = new FileStream(path, FileMode.Open))
			{
				try
				{
					var contexts = s_XmlSerializer.Deserialize(fs) as List<SetterContext>;
					if (contexts != null)
					{
						asset.m_Contexts = contexts;
					}
					var dir = path.Substring(0, path.LastIndexOf('/'));
					foreach (var ctx in contexts)
					{
						ctx.SetDirectory(dir);
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}

			return asset;
		}

		public static void Deserialize(SetterAsset asset, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				asset.m_XML = "";
				asset.m_Contexts = new List<SetterContext>();
				return;
			}
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				try
				{
					var ctx = (List<SetterContext>)s_XmlSerializer.Deserialize(ms);
					asset.m_Contexts = ctx;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					asset.m_XML = "";
					asset.m_Contexts = new List<SetterContext>();
				}
			}
		}

	}

}
