using HarmonyLib;
using System.Text;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    /*
     *      Gives credit to map makers
     *      through the Ping Tracker in
     *      the top right corner.
     */
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            LIMap? currentMap = MapLoader.CurrentMap;
            MapType mapType = MapUtils.GetCurrentMapType();

            if (mapType != MapType.LevelImposter || 
                currentMap == null)
                return;
            if (!__instance.gameObject.active)
                __instance.gameObject.SetActive(true);

            bool isFallback = MapLoader.IsFallback;
            bool isPublished = !string.IsNullOrEmpty(currentMap.authorID);
            bool isInLobby = LobbyBehaviour.Instance != null;
            bool isPingDisabled = currentMap.properties.showPingIndicator ?? false;
            if (isPingDisabled && !isFallback)
                return;

            StringBuilder pingBuilder = new();

            // Shrink all to fit
            pingBuilder.Append("<size=2.5>");

            // LevelImposter "Logo"
            if (isInLobby)
                pingBuilder.Append($"<color=#1a95d8>Level</color><color=#cb2828>Imposter</color> v{LevelImposter.Version}\n");

            // Existing Ping/Mods
            pingBuilder.Append(__instance.text.text);
            if (!__instance.text.text.EndsWith("\n"))
                pingBuilder.Append("\n");

            // Map Name
            if (isFallback && isInLobby)
                pingBuilder.Append($"<color=#1a95d8>{LIConstants.MAP_NAME}\n");
            else
                pingBuilder.Append($"<color=#1a95d8>{currentMap.name}\n");

            // Map Author
            if (isFallback && isInLobby)
                pingBuilder.Append($"<size=2>by ???</size></color>");
            else if (isPublished)
                pingBuilder.Append($"<size=2>by {currentMap.authorName}</size></color>");
            else
                pingBuilder.Append($"<size=2><i>(Freeplay Only)</i></size></color>");

            __instance.text.text = pingBuilder.ToString();
        }
    }
}
