using System;

namespace LevelImposter.Logging;

/// <summary>
///     Severity of a log message.
/// </summary>
public enum LILogLevel
{
    Debug,
    Info,
    Message,
    Warning,
    Error
}

/// <summary>
///     Emits log events throughout the mod.
///     This class has no engine dependencies so the portable map core can log as well.
///     Listeners (such as the BepInEx console sink) subscribe to <see cref="OnLog" />.
/// </summary>
public static class LILogger
{
    /// <summary>
    ///     Raised for every log message. Subscribers must never throw; a broken listener
    ///     cannot be allowed to take down map parsing.
    /// </summary>
    public static event Action<LILogLevel, object>? OnLog;

    /// <summary>
    ///     Logs a message to all subscribers
    /// </summary>
    /// <param name="logLevel">Log type/level</param>
    /// <param name="data">String or object to log</param>
    public static void Log(LILogLevel logLevel, object data)
    {
        var handlers = OnLog;
        if (handlers == null)
            return;

        foreach (var handler in handlers.GetInvocationList())
            try
            {
                ((Action<LILogLevel, object>)handler)(logLevel, data);
            }
            catch
            {
                // Logging must never throw.
            }
    }

    /// <summary>
    ///     Logs info text (gray text)
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Info(object data)
    {
        Log(LILogLevel.Info, data);
    }

    /// <summary>
    ///     Logs message text (white text)
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Msg(object data)
    {
        Log(LILogLevel.Message, data);
    }

    /// <summary>
    ///     Logs error text (red text).
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Error(object data)
    {
        Log(LILogLevel.Error, data);
    }

    /// <summary>
    ///     Logs warning text (yellow text).
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Warn(object data)
    {
        Log(LILogLevel.Warning, data);
    }

    /// <summary>
    ///     Logs debug text (gray text).
    /// </summary>
    /// <param name="data">String or object to log</param>
    public static void Debug(object data)
    {
        Log(LILogLevel.Debug, data);
    }

    /// <summary>
    ///     Logs an exception's message and stack trace.
    /// </summary>
    /// <param name="ex">The exception to log</param>
    public static void LogException(Exception ex)
    {
        Info($"Exception: {ex}\n{ex.StackTrace}");
    }
}
