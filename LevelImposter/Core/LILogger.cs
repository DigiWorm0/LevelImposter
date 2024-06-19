using BepInEx.Logging;
using System;
using UnityEngine;

namespace LevelImposter.Core
{
#pragma warning disable CS0162 // Uses constants, so ignore unreachable code warning

    /// <summary>
    /// Logs and displays data throughout the mod.
    /// </summary>
    public static class LILogger
    {
        // Set to true to log Unity Stack traces to the BepInEx console. Useful when debugging.
        private const bool LOG_UNITY_STACK_TRACE = false;

        private static ManualLogSource? _logger;

        /// <summary>
        /// Initializes LILogger instance.
        /// Ran in LevelImposter.Load()
        /// </summary>
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

        /// <summary>
        /// Repeats Unity log messages
        /// </summary>
        /// <param name="msg">Message text</param>
        /// <param name="stackTrace">Mono stack trace</param>
        /// <param name="type">Unity log type</param>
        private static void OnUnityLog(string msg, string stackTrace, LogType type)
        {
            Info($"Unity Stack Trace:\n{msg}\n{stackTrace}");
        }

        /// <summary>
        /// Logs a message to BepInEx console
        /// </summary>
        /// <param name="logLevel">Log type/level</param>
        /// <param name="data">String or object to log</param>
        public static void Log(LogLevel logLevel, object data)
        {
            _logger?.Log(logLevel, data);
        }

        /// <summary>
        /// Logs info text to BepInEx console (gray text)
        /// </summary>
        /// <param name="data">String or object to log</param>
        public static void Info(object data)
        {
            Log(LogLevel.Info, data);
        }

        /// <summary>
        /// Logs message text to BepInEx console (white text)
        /// </summary>
        /// <param name="data">String or object to log</param>
        public static void Msg(object data)
        {
            Log(LogLevel.Message, data);
        }

        /// <summary>
        /// Logs error text to BepInEx console (red text)
        /// </summary>
        /// <param name="data">String or object to log</param>
        public static void Error(object data)
        {
            Log(LogLevel.Error, data);
            Notify(data.ToString() ?? "null");
        }

        /// <summary>
        /// Logs warning text to BepInEx console (yellow text).
        /// Also renders to notifications if in Freeplay mode
        /// </summary>
        /// <param name="data">String or object to log</param>
        public static void Warn(object data)
        {
            Log(LogLevel.Warning, data);
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                Notify("<color=yellow>" + data.ToString() + "</color>");
        }

        /// <summary>
        /// Sends a message to notifications (if exists)
        /// </summary>
        /// <param name="data">String message to log</param>
        public static void Notify(string data)
        {
            if (!DestroyableSingleton<HudManager>.InstanceExists)
                return;
            NotificationPopper notifier = DestroyableSingleton<HudManager>.Instance.Notifier;
            if (notifier != null)
                notifier.AddDisconnectMessage(data);
        }
    }

#pragma warning restore CS0162
}
