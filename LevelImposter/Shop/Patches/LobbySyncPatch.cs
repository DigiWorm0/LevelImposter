using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop;

/*
 *      Synchronizes a random seed
 *      value to all connected clients
 */
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
public static class TestJoinPatch2
{
    public static void Postfix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        // Sync Random Seed
        RandomizerSync.SyncRandomSeed();

        // This is a new Lobby
        if (__instance.AmOwner)
        {
            var wasFallback = MapLoader.IsFallback;
            var isNoMap = MapLoader.CurrentMap == null;

            // If the map was a fallback or no map is currently loaded
            if (wasFallback || isNoMap)
            {
                // Choose a new random map
                MapSync.RegenerateFallbackID();
                return;
            }
        }

        // Sync the current map ID
        // TODO: Remember last map ID
        MapSync.SyncMapID();
    }
}

// [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
// public static class ClientJoinSyncPatch
// {
//     public static void Postfix()
//     {
//         RandomizerSync.SyncRandomSeed();
//         MapSync.SyncMapID();
//     }
// }