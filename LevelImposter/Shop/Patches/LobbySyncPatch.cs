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
        // TODO: Remember last map ID
        var isHost = AmongUsClient.Instance.AmHost;
        var wasFallback = MapLoader.IsFallback;
        var isNoMap = MapLoader.CurrentMap == null;

        if (isHost && (wasFallback || isNoMap))
            MapSync.RegenerateFallbackID(); // Choose a random map ID
        else
            MapSync.SyncMapID(); // Sync the current map ID

        // Always sync a new random seed
        RandomizerSync.SyncRandomSeed();
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