using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public static class LILogger
    {
        private static ManualLogSource logger;
        private const bool LOG_UNITY_STACK = false;

        public static void Init()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource("LevelImposter");
            if (LOG_UNITY_STACK)
            {
                Application.add_logMessageReceived(
                    new Action<string, string, UnityEngine.LogType>(OnUnityLog)
                );   
            }
        }

        private static void OnUnityLog(string msg, string stackTrace, UnityEngine.LogType type)
        {
            Info("Unity Stack Trace:\n" + msg + "\n" + stackTrace);
        }


        public static void Log(LogLevel logLevel, object data)
        {
            logger.Log(logLevel, data);
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
            Notify(data.ToString());
        }

        public static void Notify(string data)
        {
            if (HudManager.Instance == null)
                return;
            if (HudManager.Instance.Notifier == null)
                return;
            HudManager.Instance.Notifier.AddItem(data);
        }
    }
}
