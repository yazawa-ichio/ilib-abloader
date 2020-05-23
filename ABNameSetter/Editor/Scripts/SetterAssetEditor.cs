using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ILib.AssetBundles.NameSetter
{
	[CustomEditor(typeof(SetterAsset))]
	public class SetterAssetEditor : Editor
	{
		SetterAsset m_Asset;

		private void OnEnable()
		{
			m_Asset = target as SetterAsset;
		}

		public override void OnInspectorGUI()
		{
			GUI.enabled = true;
			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("戻す", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100)))
				{
					m_Asset.Load();
					SetterAssetCache.SetDirty();
				}
				if (GUILayout.Button("保存", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100)))
				{
					m_Asset.Save();
					SetterAssetCache.SetDirty();
				}
				if (GUILayout.Button("追加", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100)))
				{
					AddMenu();
				}
			}

			for (int i = 0; i < m_Asset.m_Contexts.Count; i++)
			{
				using (new GUILayout.VerticalScope("box", GUILayout.MinHeight(100f)))
				{
					var ctx = m_Asset.m_Contexts[i];
					var path = AssetDatabase.GetAssetPath(m_Asset);
					var dir = path.Substring(0, path.LastIndexOf('/'));
					ctx.SetDirectory(dir);
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Label(ctx.GetType().Name);
						if (GUILayout.Button("削除", GUILayout.ExpandWidth(false)))
						{
							m_Asset.m_Contexts.Remove(ctx);
							break;
						}
					}
					GUILayout.Label(ctx.Description);
					ctx.OnGUI();
				}
			}

		}

		void AddMenu()
		{
			GenericMenu menu = new GenericMenu();
			foreach (var type in Util.ContextTypes)
			{
				menu.AddItem(new GUIContent(type.Name), false, () =>
				{
					var ctx = System.Activator.CreateInstance(type) as SetterContext;
					m_Asset.m_Contexts.Add(ctx);
					SetterAssetCache.SetDirty();
				});
			}
			menu.ShowAsContext();
		}
	}
}