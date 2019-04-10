using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ILib.AssetBundles.NameSetter
{
	public class SetterAsset : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField, TextArea]
		internal string m_XML;

		internal List<SetterContext> m_Contexts = new List<SetterContext>();

		public void OnAfterDeserialize()
		{
			Serializer.Deserialize(this, m_XML);
		}

		public void OnBeforeSerialize()
		{
			m_XML = Serializer.Serialize(this);
		}

		public void Save()
		{
			var path = AssetDatabase.GetAssetPath(this);
			Serializer.Serialize(this, path);
			AssetDatabase.ImportAsset(path);
		}

		public void Load()
		{
			var path = AssetDatabase.GetAssetPath(this);
			Serializer.Deserialize(this, System.IO.File.ReadAllText(path));
			var dir = path.Substring(0, path.LastIndexOf('/'));
			foreach (var ctx in m_Contexts)
			{
				ctx.SetDirectory(dir);
			}
		}

	}

}
