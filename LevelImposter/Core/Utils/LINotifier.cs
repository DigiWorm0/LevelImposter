namespace LevelImposter.Core.Utils;

/// <summary>
///     Sends in-game notifications through the HUD.
///     Kept separate from <see cref="LILogger" /> so logging stays Unity-free.
/// </summary>
public static class LINotifier
{
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
