using LevelImposter.Core.Utils;

namespace LevelImposter.Logging;

/// <summary>
///     Sends in-game notifications through the HUD.
///     Kept separate from <see cref="LILogger" /> so logging stays Unity-free.
/// </summary>
public static class NotifierLogSink
{
    /// <summary>
    ///     Subscribes in-game notifications to <see cref="LILogger.OnLog" />.
    ///     Ran in LevelImposter.Load().
    /// </summary>
    public static void Init()
    {
        // Avoid double subscription if the plugin is hot-reloaded
        LILogger.OnLog -= OnLog;
        LILogger.OnLog += OnLog;
    }

    private static void OnLog(LILogLevel level, object data)
    {
        // Notifications are only rendered during Freeplay
        if (!GameState.IsInFreeplay)
            return;

        switch (level)
        {
            case LILogLevel.Error:
                Notify(data.ToString() ?? "null");
                break;
            case LILogLevel.Warning:
                Notify("<color=yellow>" + data + "</color>");
                break;
        }
    }

    /// <summary>
    ///     Sends a message to notifications (if exists)
    /// </summary>
    /// <param name="data">Message to display</param>
    /// <param name="isError">
    ///     If true, will be displayed as an error message. If false, will be displayed as a non-error
    ///     message.
    /// </param>
    public static void Notify(string data, bool isError = true)
    {
        // Check HudManager exists
        if (!DestroyableSingleton<HudManager>.InstanceExists)
            return;

        // Get notifier
        var notifier = DestroyableSingleton<HudManager>.Instance.Notifier;
        if (notifier == null)
            return;

        // Error
        if (isError)
        {
            notifier.AddDisconnectMessage(data);
        }
        // Non-error
        else
        {
            var notifyText = "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + data + "</font>";
            notifier.SettingsChangeMessageLogic(StringNames.None, notifyText, false);
        }
    }
}
