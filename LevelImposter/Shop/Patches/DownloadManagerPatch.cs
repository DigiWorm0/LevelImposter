using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      Remove player from DownloadManager
     *      if the player disconnects
     */
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    public static class DisconnectPatch
    {
        public static void Postfix([HarmonyArgument(0)]InnerNet.ClientData data)
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
        private static bool _isDownloadTextDisplayed = false;
        public static void Postfix(GameStartManager __instance)
        {
            if (!DownloadManager.CanStart)
            {
                __instance.StartButton.color = Palette.DisabledClear;
                __instance.startLabelText.color = Palette.DisabledClear;
                __instance.GameStartText.text = DownloadManager.GetStartText();
                _isDownloadTextDisplayed = true;
            }
            else if (_isDownloadTextDisplayed)
            {
                __instance.StartButton.color = Palette.EnabledColor;
                __instance.startLabelText.color = Palette.EnabledColor;
                __instance.GameStartText.text = string.Empty;
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
}
