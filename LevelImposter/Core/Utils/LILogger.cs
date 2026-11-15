using System;
using BepInEx.Logging;

namespace LevelImposter.Core.Utils;

/// <summary>
///     Logs data throughout the mod.
///     This class is Unity-free so the portable map core can log as well.
/// </summary>
public static class LILogger
{
    private static ManualLogSource? _logger;

    /// <summary>
    ///     Initializes LILogger instance.
    ///     Ran in LevelImposter.Load()
    /// </summary>
    public static void Init()
    {
        _logger = BepInEx.Logging.Logger.CreateLogSource("LevelImposter");
    }

    /// <summary>
    ///     Logs a message to BepInEx console
    /// </summary>
    /// <param name="logLevel">Log type/level</param>
    /// <param name="data">String or object to log</param>
    public static void Log(LogLevel logLevel, object data)
    {
        _logger?.Log(logLevel, data);
    }

    /// <summary>
    ///     Logs info text to BepInEx console (gray text)
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Info(object data)
    {
        Log(LogLevel.Info, data);
    }

    /// <summary>
    ///     Logs message text to BepInEx console (white text)
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Msg(object data)
    {
        Log(LogLevel.Message, data);
    }

    /// <summary>
    ///     Logs error text to BepInEx console (red text).
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Error(object data)
    {
        Log(LogLevel.Error, data);
    }

    /// <summary>
    ///     Logs warning text to BepInEx console (yellow text).
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Warn(object data)
    {
        Log(LogLevel.Warning, data);
    }

    /// <summary>
    ///     Logs debug text to BepInEx console (gray text).
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Debug(object data)
    {
        Log(LogLevel.Debug, data);
    }

    /// <summary>
    ///     Logs an exception's message and stack trace to BepInEx console.
    /// </summary>
    /// <param name="ex">The exception to log</param>
    public static void LogException(Exception ex)
    {
        Info($"Exception: {ex}\n{ex.StackTrace}");
    }
}
