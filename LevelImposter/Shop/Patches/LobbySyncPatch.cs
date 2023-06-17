using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    /*
     *      Synchronizes a random seed
     *      value to all connected clients
     */
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static class LobbyStartSyncPatch
    {
        public static void Postfix()
        {
            RandomizerSync.SyncRandomSeed();

            string? lastMapID = ConfigAPI.GetLastMapID();
            bool hasLastMap = lastMapID != null && (MapFileAPI.Instance?.Exists(lastMapID) ?? false);
            bool isHost = AmongUsClient.Instance.AmHost;

            if (MapLoader.CurrentMap == null && lastMapID != null && hasLastMap && isHost)
                MapLoader.LoadMap(lastMapID, false, MapSync.SyncMapID);
            else if (MapLoader.IsFallback || MapLoader.CurrentMap == null)
                MapSync.RegenerateFallbackID();
            else
                MapSync.SyncMapID();
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    public static class ClientJoinSyncPatch
    {
        public static void Postfix()
        {
            RandomizerSync.SyncRandomSeed();
            MapSync.SyncMapID();
        }
    }
}