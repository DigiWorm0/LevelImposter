using HarmonyLib;

namespace LevelImposter.Shop
{
    /*
     *      Remove player from DownloadManager
     *      if the player disconnects
     */
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    public static class DisconnectPatch
    {
        public static void Postfix([HarmonyArgument(0)] InnerNet.ClientData data)
        {
            if (data.Character != null)
                DownloadManager.RemovePlayer(data.Character);
        }
    }

    /*
    *      Updates the lobby menu
    *      text w/ DownloadManager
    */
    /*
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class LobbyTextPatch
    {
        private static bool _isDownloadTextDisplayed = false;
        public static void Postfix(GameStartManager __instance)
        {
            // TODO: Fix this
            if (!DownloadManager.CanStart)
            {
                __instance.StartButton.SetButtonEnableState(false);
                __instance.GameStartText.text = DownloadManager.GetStartText();
                _isDownloadTextDisplayed = true;
            }
            else if (_isDownloadTextDisplayed)
            {
                __instance.LastPlayerCount = -1; // Forces GameStartManager.Update to update the button state
                __instance.GameStartText.text = string.Empty;
                _isDownloadTextDisplayed = false;
            }
        }
    }
    */

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
}
