using BepInEx.Logging;

namespace LevelImposter.Logging;

/// <summary>
///     Forwards <see cref="LILogger" /> events to the BepInEx console.
///     Ran in LevelImposter.Load().
/// </summary>
public static class BepInExLogSink
{
    private static ManualLogSource? _logSource;

    /// <summary>
    ///     Creates the BepInEx log source and subscribes it to <see cref="LILogger.OnLog" />.
    /// </summary>
    public static void Init()
    {
        _logSource = BepInEx.Logging.Logger.CreateLogSource("LevelImposter");

        // Avoid double subscription if the plugin is hot-reloaded
        LILogger.OnLog -= OnLog;
        LILogger.OnLog += OnLog;
    }

    private static void OnLog(LILogLevel level, object data)
    {
        _logSource?.Log(ToBepInExLevel(level), data);
    }

    private static LogLevel ToBepInExLevel(LILogLevel level)
    {
        return level switch
        {
            LILogLevel.Debug => LogLevel.Debug,
            LILogLevel.Message => LogLevel.Message,
            LILogLevel.Warning => LogLevel.Warning,
            LILogLevel.Error => LogLevel.Error,
            _ => LogLevel.Info
        };
    }
}
