using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter
{
    static class LILogger
    {
        private const bool PRINT_STACK = true;
        private static ManualLogSource logger;

        public static void Init()
        {
            logger = Logger.CreateLogSource("LevelImposter");
            UnityEngine.Application.add_logMessageReceived(
                new Action<string, string, UnityEngine.LogType>(OnUnityLog)
            );
        }

        private static void OnUnityLog(string msg, string stackTrace, UnityEngine.LogType type)
        {
            if (PRINT_STACK)
            {
                LogInfo("Unity Stack Trace:\n" + stackTrace);
            }
        }

        public static void Log(LogLevel logLevel, object data)
        {
            logger.Log(logLevel, data);
        }

        public static void LogInfo(object data)
        {
            Log(LogLevel.Info, data);
        }

        public static void LogMsg(object data)
        {
            Log(LogLevel.Message, data);
        }

        public static void LogError(object data)
        {
            Log(LogLevel.Error, data);
        }
    }
}
