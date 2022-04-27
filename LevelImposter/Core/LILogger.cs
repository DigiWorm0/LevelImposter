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

        public static void Init()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource("LevelImposter");
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
        }

        public static void Warn(object data)
        {
            Log(LogLevel.Warning, data);
        }
    }
}
