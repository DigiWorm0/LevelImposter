using HarmonyLib;
using LevelImposter.Core;
using LevelImposter.FileIO;

namespace LevelImposter.Shop;

/*
 *      Synchronizes a random seed
 *      value to all connected clients
 */
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class ClientJoinSyncPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        // Another player has joined the lobby (not me)
        if (!__instance.AmOwner)
        {
            MapSync.SyncMapID();
            return;
        }
        
        // Check if an existing map is loaded
        var isLISelected = GameConfiguration.CurrentMapType == MapType.LevelImposter;
        var isCustomMapLoaded = GameConfiguration.CurrentMap != null;
        var isRandomized = GameConfiguration.HideMapName || !isLISelected;  // <-- If not LevelImposter, treat as randomized
        
        if (isCustomMapLoaded && !isRandomized)
            return; // <-- A proper custom map is already loaded

        // Attempt to load the last selected map
        if (!isRandomized)
        {
            var lastMapID = ConfigAPI.GetLastMapID();
            var lastMap = lastMapID != null ? MapFileAPI.Get(lastMapID) : null;
            if (lastMap != null)
            {
                GameConfiguration.SetMap(lastMap);
                LILogger.Info($"Loaded last selected map [{lastMapID}] for host.");
                return;
            }
        }
        
        // Otherwise, attempt to randomize map
        MapRandomizer.RandomizeMap(false);
        
        // Fallback to Skeld if no map is loaded (and we're on LevelImposter)
        if (GameConfiguration.CurrentMap == null && isLISelected)
        {
            LILogger.Warn("No custom maps available, falling back to Skeld.");
            GameConfiguration.SetMapType(MapType.Skeld);
        }
    }
}