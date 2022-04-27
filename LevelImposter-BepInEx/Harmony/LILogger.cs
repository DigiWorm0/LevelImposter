using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter
{
    static class LILogger
    {
        private static ManualLogSource logger;
        private const bool PRINT_STACK_TRACE = true;

        public static void Init()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource("LevelImposter");
            var debug = MainHarmony.ConfigFile.Bind("Debug", "ShowUnityLogs", false);
            if (debug.Value)
            {
                UnityEngine.Application.add_logMessageReceived(
                    new Action<string, string, UnityEngine.LogType>(OnUnityLog)
                );   
            }
        }

        private static void OnUnityLog(string msg, string stackTrace, UnityEngine.LogType type)
        {
            if ((Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.P)) || PRINT_STACK_TRACE)
            {
                LogInfo("Unity Stack Trace:\n" + msg + "\n" + stackTrace);
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

        public static void LogWarn(object data)
        {
            Log(LogLevel.Warning, data);
        }
    }
}
