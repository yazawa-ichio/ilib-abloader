using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace ILib.AssetBundles.ExManifest
{
	public class StashFileScope : System.IDisposable
	{
		string[] m_Files;
		string m_Suffix;
		bool m_DeleteTmp;

		public StashFileScope(string[] files, string suffix = "_tmp", bool deleteTmp = true)
		{
			m_Files = files;
			m_Suffix = suffix;
			foreach (var file in m_Files)
			{
				File.Copy(file, file + m_Suffix, true);
			}
			m_DeleteTmp = deleteTmp;
		}

		public void Dispose()
		{
			foreach (var file in m_Files)
			{
				File.Copy(file + m_Suffix, file, true);
			}
			if (m_DeleteTmp)
			{
				foreach (var file in m_Files)
				{
					File.Delete(file + m_Suffix);
				}
			}
		}

	}
}