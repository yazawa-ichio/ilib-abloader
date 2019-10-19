using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace ILib.AssetBundles.Logger
{
	public static class Log
	{
		const string ENABLED_DEFINE = "ILIB_ABLOADER_LOG_ENABLED";

		public enum LogLevel
		{
			Exception,
			Error,
			Warning,
			Debug,
			Trace,
			All = Trace,
		}

		static ILogger s_Logger;
		static bool s_Enabled = true;
		static bool s_EnabledAssert = true;
#if DEBUG
		static LogLevel s_Level = LogLevel.Debug;
#else
		static LogLevel s_Level = LogLevel.Warning;
#endif

		public static bool Enabled
		{
			get => s_Enabled;
			set => s_Enabled = value;
		}

		public static bool EnabledAssert
		{
			get => s_EnabledAssert;
			set => s_EnabledAssert = value;
		}

		public static LogLevel Level
		{
			get => s_Level;
			set => s_Level = value;
		}

		public static ILogger Logger
		{
			get => s_Logger;
			set => s_Logger = value;
		}

		internal static void Init()
		{
			s_Logger = s_Logger ?? UnityEngine.Debug.unityLogger;
		}

		[Conditional("UNITY_EDITOR"), Conditional(ENABLED_DEFINE)]
		internal static void Trace(string message)
		{
			if (!s_Enabled || s_Level < LogLevel.Trace) return;
			s_Logger?.Log(LogType.Log, message);
		}

		[Conditional("UNITY_EDITOR"), Conditional(ENABLED_DEFINE)]
		internal static void Trace<T1>(string message, T1 arg0)
		{
			if (!s_Enabled || s_Level < LogLevel.Trace) return;
			s_Logger?.Log(LogType.Log, string.Format(message, arg0));
		}

		[Conditional("UNITY_EDITOR"), Conditional(ENABLED_DEFINE)]
		internal static void Trace<T1, T2>(string message, T1 arg0, T2 arg1)
		{
			if (!s_Enabled || s_Level < LogLevel.Trace) return;
			s_Logger?.Log(LogType.Log, string.Format(message, arg0, arg1));
		}

		[Conditional("UNITY_EDITOR"), Conditional(ENABLED_DEFINE)]
		internal static void Trace<T1, T2, T3>(string message, T1 arg0, T2 arg1, T3 arg2)
		{
			if (!s_Enabled || s_Level < LogLevel.Trace) return;
			s_Logger?.Log(LogType.Log, string.Format(message, arg0, arg1, arg2));
		}

		[Conditional("DEBUG"), Conditional(ENABLED_DEFINE)]
		internal static void Debug(string message)
		{
			if (!s_Enabled || s_Level < LogLevel.Debug) return;
			s_Logger?.Log(LogType.Log, message);
		}

		[Conditional("DEBUG"), Conditional(ENABLED_DEFINE)]
		internal static void Debug<T1>(string message, T1 arg0)
		{
			if (!s_Enabled || s_Level < LogLevel.Debug) return;
			s_Logger?.Log(LogType.Log, string.Format(message, arg0));
		}

		[Conditional("DEBUG"), Conditional(ENABLED_DEFINE)]
		internal static void Debug<T1, T2>(string message, T1 arg0, T2 arg1)
		{
			if (!s_Enabled || s_Level < LogLevel.Debug) return;
			s_Logger?.Log(LogType.Log, string.Format(message, arg0, arg1));
		}

		[Conditional("DEBUG"), Conditional(ENABLED_DEFINE)]
		internal static void Debug<T1, T2, T3>(string message, T1 arg0, T2 arg1, T3 arg2)
		{
			if (!s_Enabled || s_Level < LogLevel.Debug) return;
			s_Logger?.Log(LogType.Log, string.Format(message, arg0, arg1, arg2));
		}

		internal static void Warning(string message)
		{
			if (!s_Enabled || s_Level < LogLevel.Warning) return;
			s_Logger?.Log(LogType.Warning, message);
		}

		internal static void Warning<T1>(string message, T1 arg0)
		{
			if (!s_Enabled || s_Level < LogLevel.Warning) return;
			s_Logger?.Log(LogType.Warning, string.Format(message, arg0));
		}

		internal static void Warning<T1, T2>(string message, T1 arg0, T2 arg1)
		{
			if (!s_Enabled || s_Level < LogLevel.Warning) return;
			s_Logger?.Log(LogType.Warning, string.Format(message, arg0, arg1));
		}

		internal static void Error(string message)
		{
			if (!s_Enabled || s_Level < LogLevel.Error) return;
			s_Logger?.Log(LogType.Warning, message);
		}

		internal static void Error<T1>(string message, T1 arg0)
		{
			if (!s_Enabled || s_Level < LogLevel.Error) return;
			s_Logger?.Log(LogType.Warning, string.Format(message, arg0));
		}

		internal static void Error<T1, T2>(string message, T1 arg0, T2 arg1)
		{
			if (!s_Enabled || s_Level < LogLevel.Error) return;
			s_Logger?.Log(LogType.Warning, string.Format(message, arg0, arg1));
		}

		internal static void Exception(System.Exception ex)
		{
			if (!s_Enabled || s_Level < LogLevel.Exception) return;
			s_Logger?.LogException(ex);
		}

		[Conditional("DEBUG"), Conditional("ILIB_ASSETBUNDLE_ENABLED_LOG")]
		internal static void Assert(bool condition)
		{
			if (!s_EnabledAssert) return;
			UnityEngine.Debug.Assert(condition);
		}

		[Conditional("DEBUG"), Conditional("ILIB_ASSETBUNDLE_ENABLED_LOG")]
		internal static void Assert(bool condition, string message)
		{
			if (!s_EnabledAssert) return;
			UnityEngine.Debug.Assert(condition, message);
		}

		[Conditional("DEBUG"), Conditional("ILIB_ASSETBUNDLE_ENABLED_LOG")]
		internal static void Assert<T1>(bool condition, string message, T1 arg0)
		{
			if (!s_EnabledAssert) return;
			UnityEngine.Debug.Assert(condition, string.Format(message, arg0));
		}

	}
}
