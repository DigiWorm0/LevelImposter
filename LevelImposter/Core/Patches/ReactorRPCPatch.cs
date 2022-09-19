using System;
using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;
using Reactor.Networking.MethodRpc;

namespace LevelImposter.Core
{
    /*
     *      Transmits the LI map ID over RPC
     */
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public static class SendRpcPatch
    {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists || PlayerControl.LocalPlayer == null)
                return;

            Guid mapID = Guid.Empty;
            if (MapLoader.currentMap != null)
                Guid.TryParse(MapLoader.currentMap.id, out mapID);
            string mapIDStr = mapID.ToString();

            LILogger.Info("[RPC] Transmitting map ID [" + mapIDStr + "]");
            ReactorRPC.RPCSendMapID(PlayerControl.LocalPlayer, mapIDStr);
        }
    }

    public static class ReactorRPC
    {
        public static Guid? downloadingMapID = null;
        public const int RPC_ID = 99; // <100 for TOU Support

        [MethodRpc(RPC_ID)]
        public static void RPCSendMapID(PlayerControl _p, string mapIDStr)
        {
            if (AmongUsClient.Instance.AmHost)
                return;
            LILogger.Info("[RPC] Received map ID [" + mapIDStr + "]");

            // Parse ID
            Guid mapID;
            if (!Guid.TryParse(mapIDStr, out mapID))
            {
                LILogger.Error("Invalid map ID");
            }

            // Get Current
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
            else if (currentMapID == mapIDStr || downloadingMapID == mapID)
            {
                return;
            }
            else if (MapFileAPI.Instance.Exists(mapIDStr))
            {
                MapLoader.LoadMap(mapIDStr);
            }
            else
            {
                downloadingMapID = mapID;
                LILogger.Notify("<color=#1a95d8>Downloading map, please wait...</color>");
                LevelImposterAPI.Instance.DownloadMap(mapID, ((LIMap map) =>
                {
                    if (downloadingMapID == mapID)
                    {
                        MapLoader.LoadMap(mapID.ToString());
                        LILogger.Notify("<color=#1a95d8>Download finished!</color>");
                        downloadingMapID = null;
                    }
                }));
            }
        }
    }
}
