using System;
using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;
using Hazel;

namespace LevelImposter.Core
{
    /*
     *        (Legacy RPC Methods)
     *      |======================|
     *      | IN CASE OF EMERGENCY |
     *      |     BREAK GLASS      |
     *      |======================|
     */

    /*
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public static class SendRpcPatch
    {
        public const int RPC_ID = 99;

        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists)
                return;

            // Parse ID
            Guid mapID = Guid.Empty;
            if (MapLoader.currentMap != null)
                Guid.TryParse(MapLoader.currentMap.id, out mapID);
            LILogger.Info("[RPC] Transmitting map ID [" + mapID.ToString() + "]");

            // Transmit ID
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, RPC_ID);
            messageWriter.Write(mapID.ToByteArray());
            messageWriter.EndMessage();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class ReceiveRpcPatch
    {
        public static Guid? downloadingMapID = null;

        public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId != SendRpcPatch.RPC_ID)
                return;

            // Parse ID
            byte[] mapBytes = reader.ReadBytes(16);
            Guid mapID = new Guid(mapBytes);
            string mapIDStr = mapID.ToString();
            LILogger.Info("[RPC] Received map ID [" + mapIDStr + "]");

            string currentMapID = MapLoader.currentMap == null ? "" : MapLoader.currentMap.id;

            if (downloadingMapID != null)
            {
                LILogger.Notify("Download stopped.");
                downloadingMapID = null;
            }

            // Handle ID
            if (mapID.Equals(Guid.Empty))
            {
                MapLoader.UnloadMap();
            }
            else if (currentMapID.Equals(mapIDStr))
            {
                return;
            }
            else if (MapLoader.Exists(mapIDStr))
            {
                MapLoader.LoadMap(mapIDStr);
            }
            else
            {
                downloadingMapID = mapID;
                LILogger.Notify("<color=#1a95d8>Downloading map, please wait...</color>");
                MapAPI.DownloadMap(mapID, ((string mapJson) =>
                {
                    if (downloadingMapID == mapID)
                    {
                        MapLoader.WriteMap(mapID.ToString(), mapJson);
                        MapLoader.LoadMap(mapID.ToString());
                        LILogger.Notify("<color=#1a95d8>Download finished!</color>");
                        downloadingMapID = null;
                    }
                }));
            }
        }
    }
    */
}
