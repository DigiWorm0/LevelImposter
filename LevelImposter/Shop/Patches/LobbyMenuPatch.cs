using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    /*
     *      Replace Map Name in Options Console.
     *      Also skips Submerged, if it isn't present.
     */
    /*
    [HarmonyPatch(typeof(KeyValueOption))]
    public static class MapNameValuePatch
    {
        /// <summary>
        /// Removes all placeholder or unavailable maps
        /// </summary>
        /// <param name="__instance">KeyValueOption Instance</param>
        private static void RemoveElems(KeyValueOption __instance)
        {
            bool isMapLoaded = MapLoader.CurrentMap != null;
            for (int i = __instance.Values.Count - 1; i >= 0; i--)
            {
                string mapName = __instance.Values[i].Key;
                bool isPlaceholder = string.IsNullOrEmpty(mapName);
                bool isUnavailable = !isMapLoaded && mapName == LIConstants.MAP_NAME;
                if (isPlaceholder || isUnavailable)
                {
                    __instance.Values.RemoveAt(i);
                    if (__instance.Selected >= __instance.Values.Count)
                        __instance.Selected--;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(KeyValueOption.FixedUpdate))]
        public static bool UpdateFix(KeyValueOption __instance)
        {
            bool isMapTitle = __instance.Title == StringNames.GameMapName;
            bool hasChanged = __instance.oldValue != __instance.Selected;
            bool shouldShowName = __instance.Selected == (int)MapType.LevelImposter || !MapLoader.IsFallback;
            bool isMapLoaded = MapLoader.CurrentMap != null;

            if (isMapTitle && hasChanged)
            {
                RemoveElems(__instance);
                if (shouldShowName && isMapLoaded)
                {
                    __instance.oldValue = __instance.Selected;

                    
                    if (MapLoader.IsFallback) // Random Map
                        __instance.ValueText.text = LIConstants.MAP_NAME;
                    else // Map Name
                        __instance.ValueText.text = $"{MapLoader.CurrentMap?.name}\n<size=0.9>by {MapLoader.CurrentMap?.authorName}";

                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(KeyValueOption.Increase))]
        [HarmonyPatch(nameof(KeyValueOption.Decrease))]
        public static void IncrementFix(KeyValueOption __instance)
        {
            if (__instance.Title == StringNames.GameMapName)
            {
                RemoveElems(__instance);
                ConfigAPI.SetLastMapID(null);

                if (!MapLoader.IsFallback || MapLoader.CurrentMap == null)
                    MapSync.RegenerateFallbackID();
                else
                    MapSync.SyncMapID();
            }
        }
    }
    */

    // TODO: Fix Lobby Map Selection

    /*
     *      Initializes a new Map Console in the Lobby
     */
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static class LobbyMenuInitPatch
    {
        public static void Postfix()
        {
            LobbyConsoleBuilder.Build();
        }
    }

    /*
     *      Replaces the LI map name with the actual map name
     */
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    public static class StringRenamePatch
    {
        public static void Postfix(ref string __result)
        {
            if (MapLoader.CurrentMap == null || MapLoader.IsFallback)
                return;

            __result = __result.Replace(LIConstants.MAP_NAME, MapLoader.CurrentMap.name);
        }
    }
}
