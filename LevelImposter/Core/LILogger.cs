using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Logs and displays data throughout the mod.
    /// </summary>
    public static class LILogger
    {
        // Set to true to log Unity Stack traces to the BepInEx console. Useful when debugging.
        private const bool LOG_UNITY_STACK_TRACE = false;

        private static ManualLogSource _logger;

        public static void Init()
        {
            _logger = BepInEx.Logging.Logger.CreateLogSource("LevelImposter");
            if (LOG_UNITY_STACK_TRACE)
            {
                Application.add_logMessageReceived(
                    new Action<string, string, LogType>(OnUnityLog)
                );   
            }
        }

        private static void OnUnityLog(string msg, string stackTrace, LogType type)
        {
            Info("Unity Stack Trace:\n" + msg + "\n" + stackTrace);
        }

        public static void Log(LogLevel logLevel, object data)
        {
            _logger.Log(logLevel, data);
        }

        public static void Info(object data)
        {
            Log(LogLevel.Info, data);
        }

        public static void Msg(object data)
        {
            Log(LogLevel.Message, data);
        }

        public static void Error(object data)
        {
            Log(LogLevel.Error, data);
            Notify(data.ToString());
        }

        public static void Warn(object data)
        {
            Log(LogLevel.Warning, data);
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                Notify("<color=yellow>" + data.ToString() + "</color>");
        }

        public static void Notify(string data)
        {
            if (!DestroyableSingleton<HudManager>.InstanceExists)
                return;
            NotificationPopper notifier = DestroyableSingleton<HudManager>.Instance.Notifier;
            if (notifier != null)
                notifier.AddItem(data);
        }
    }
}
