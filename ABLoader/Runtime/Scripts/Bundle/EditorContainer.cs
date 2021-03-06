﻿#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ILib.AssetBundles
{
	using Logger;

	public class EditorContainer : IBundleContainer
	{
		string m_Name;
		public EditorContainer(string name)
		{
			Log.Trace("[ilib-abloader] create EditorContainer {0}.", name);
			m_Name = name;
		}

		public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
		{
			Log.Trace("[ilib-abloader] EditorContainer {0}, LoadAsset<{1}>({2}).", m_Name, typeof(T), assetName);
			var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(m_Name, assetName);
			return paths.Length > 0 ? AssetDatabase.LoadAssetAtPath<T>(paths[0]) : null;
		}

		public void LoadAssetAsync<T>(string assetName, Action<T> onSuccess) where T : UnityEngine.Object
		{
			onSuccess?.Invoke(LoadAsset<T>(assetName));
		}

		public void LoadScene(string sceneName, LoadSceneMode mode)
		{
			Log.Trace("[ilib-abloader] EditorContainer {0}, LoadScene {1}. mode{2}", m_Name, sceneName, mode);
			var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(m_Name, sceneName);
			if (paths.Length == 0) return;
			LoadSceneParameters parameters = new LoadSceneParameters(mode);
			EditorSceneManager.LoadSceneInPlayMode(paths[0], parameters);
		}

		public void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action onSuccess)
		{
			Log.Trace("[ilib-abloader] EditorContainer {0}, LoadSceneAsync {1}. mode{2}", m_Name, sceneName, mode);
			var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(m_Name, sceneName);
			if (paths.Length == 0)
			{
				onSuccess?.Invoke();
				return;
			}
			LoadSceneParameters parameters = new LoadSceneParameters(mode);
			EditorSceneManager.LoadSceneAsyncInPlayMode(paths[0], parameters);

			//Asyncのコールバックが正常に動かないので無理やり実装
			var scene = EditorSceneManager.GetSceneByPath(paths[0]);
			EditorApplication.CallbackFunction onLoad = null;
			onLoad = () =>
			{
				if (scene.isLoaded || !scene.IsValid())
				{
					EditorApplication.update -= onLoad;
					onSuccess?.Invoke();
				}
			};
			EditorApplication.update += onLoad;
		}

		public void RemoveRef()
		{

		}

		public void SetUnloadAll(bool unloadAll, bool depend = false)
		{

		}
	}

}
#endif