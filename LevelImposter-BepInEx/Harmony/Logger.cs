using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter
{
    static class LILogger
    {
        static ManualLogSource logger;

        public static void Init()
        {
            logger = Logger.CreateLogSource("LevelImposter");
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
