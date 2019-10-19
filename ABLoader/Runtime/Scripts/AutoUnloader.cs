using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.AssetBundles
{
	using Logger;

	public class AutoUnloader 
	{
		class AutoUnloaderUpdater : MonoBehaviour
		{
			AutoUnloader m_Instance = new AutoUnloader();

			private void Update()
			{
				m_Instance.Update();
			}

		}

		public static float UnloadCycle = 2f;
		public static bool Pause = false;

		static AutoUnloaderUpdater s_Updater;

		public static void ChangeMode(UnloadMode mode)
		{
			Log.Trace("[ilib-abloader]AutoUnloader change mode:{0}", mode);

			if (mode != UnloadMode.Auto)
			{
				if (s_Updater != null)
				{
					GameObject.Destroy(s_Updater.gameObject);
					s_Updater = null;
				}
			}
			else if (s_Updater == null)
			{
				GameObject obj = new GameObject("ILib.ABLoader.AutoUnloader");
				GameObject.DontDestroyOnLoad(obj);
				obj.hideFlags = HideFlags.DontSave;
				s_Updater = obj.AddComponent<AutoUnloaderUpdater>();
			}
		}


		private AutoUnloader() { }

		float m_Time;

		void Update()
		{
			if (Pause)
			{
				return;
			}
			m_Time += Time.unscaledDeltaTime;
			if (m_Time > UnloadCycle)
			{
				ABLoader.Unload();
				m_Time = 0;
			}
		}

	}
}
