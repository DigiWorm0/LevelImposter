namespace LevelImposter.Shop
{
    /*
     *      Gives credit to map makers
     *      through the Ping Tracker in
     *      the top right corner.
     */
    /*
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

            // Existing Ping/Mods
            pingBuilder.Append(__instance.text.text);
            if (!__instance.text.text.EndsWith("\n"))
                pingBuilder.Append("\n");

            // LevelImposter "Logo"
            if (isInLobby)
                pingBuilder.Append($"<size=2><color=#1a95d8>Level</color><color=#cb2828>Imposter</color> v{LevelImposter.DisplayVersion}</size>\n");

            // Map Name
            pingBuilder.Append($"<size=2><color=#1a95d8><b>{mapName}</b></color></size>");

            // Map Author
            if (isFallback && isInLobby)
                pingBuilder.Append("");
            else if (isPublished)
                pingBuilder.Append($"\n<size=2><color=#2e7296>by {currentMap.authorName}</color></size>");
            else
                pingBuilder.Append($"\n<size=2><i>(Freeplay Only)</i></size>");


            __instance.text.text = pingBuilder.ToString();
        }
    }
    */
    // TODO: Implement new map credit system
}
