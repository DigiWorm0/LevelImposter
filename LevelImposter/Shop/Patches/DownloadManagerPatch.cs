using HarmonyLib;
using InnerNet;

namespace LevelImposter.Shop;

/*
 *      Remove player from DownloadManager
 *      if the player disconnects
 */
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public static class DisconnectPatch
{
    public static void Postfix([HarmonyArgument(0)] ClientData data)
    {
        if (data.Character != null)
            DownloadManager.RemovePlayer(data.Character);
    }
}

/*
 *      Updates the lobby menu
 *      text w/ DownloadManager
 */
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class LobbyTextPatch
{
    private static bool _isDownloadTextDisplayed;

    public static void Postfix(GameStartManager __instance)
    {
        if (!DownloadManager.CanStart)
        {
            // Disable Button
            __instance.StartButton.SetButtonEnableState(false);

            // Set Text
            var buttonText = DownloadManager.GetStartText();
            __instance.GameStartTextClient.text = buttonText; // Client
            __instance.StartButton.ChangeButtonText(buttonText); // Host

            // Set flag
            _isDownloadTextDisplayed = true;
        }

        // Check flag to prevent unnecessary updates
        else if (_isDownloadTextDisplayed)
        {
            // Force GameStartManager.Update to update the button state
            __instance.LastPlayerCount = -1;

            // Clear Text
            __instance.GameStartTextClient.text = string.Empty;

            // Clear flag
            _isDownloadTextDisplayed = false;
        }
    }
}

/*
 *      Disables the Start Button
 *      when the map isn't downloaded
 */
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
public static class LobbyStartPatch
{
    public static bool Prefix()
    {
        return DownloadManager.CanStart;
    }
}