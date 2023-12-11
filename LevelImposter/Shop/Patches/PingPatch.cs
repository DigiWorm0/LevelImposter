using HarmonyLib;
using LevelImposter.Core;
using System.Text;

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

            if (mapType != MapType.LevelImposter || currentMap == null)
                return;
            if (!__instance.gameObject.active)
                __instance.gameObject.SetActive(true);

            bool isPublished = !string.IsNullOrEmpty(currentMap.authorID);
            bool isFallback = GameState.IsFallbackMapLoaded;
            bool isInLobby = GameState.IsInLobby;
            bool isPingDisabled = currentMap.properties.showPingIndicator ?? false;
            string mapName = GameState.MapName;
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
            pingBuilder.Append($"<color=#1a95d8>{mapName}\n");

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
