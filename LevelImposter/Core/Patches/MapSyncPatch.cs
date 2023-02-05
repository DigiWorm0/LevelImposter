using System;
using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;
using Reactor.Networking.Attributes;

namespace LevelImposter.Core
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    public static class MapSyncPatch
    {
        public static void Postfix()
        {
            MapUtils.SyncMapID();
        }
    }

    public static class MapSync
    {
        private static Guid? _activeDownloadingID = null;

        [MethodRpc((uint)LIRpc.SyncMapID)]
        public static void RPCSendMapID(PlayerControl _, string mapIDStr)
        {
            if (GameStartManager.Instance != null)
                GameStartManager.Instance.ResetStartState();
            DownloadManager.Reset();
            if (AmongUsClient.Instance.AmHost)
                return;
            LILogger.Info($"[RPC] Received map ID [{mapIDStr}]");

            // Parse ID
            bool isSuccess = Guid.TryParse(mapIDStr, out Guid mapID);
            if (!isSuccess)
            {
                LILogger.Error($"Invalid map ID [{mapIDStr}]");
                return;
            }

            // Get Current
            string currentMapID = MapLoader.CurrentMap == null ? "" : MapLoader.CurrentMap.id;
            if (_activeDownloadingID != null)
            {
                LILogger.Notify("Download stopped.");
                _activeDownloadingID = null;
            }

            // Handle ID
            if (mapID.Equals(Guid.Empty))
            {
                MapLoader.UnloadMap();
            }
            else if (currentMapID == mapIDStr || _activeDownloadingID == mapID)
            {
                return;
            }
            else if (MapFileAPI.Instance?.Exists(mapIDStr) == true)
            {
                MapLoader.LoadMap(mapIDStr, null);
            }
            else
            {
                _activeDownloadingID = mapID;
                LILogger.Notify("<color=#1a95d8>Downloading map, please wait...</color>");
                DownloadManager.StartDownload();
                LevelImposterAPI.Instance?.DownloadMap(mapID, (LIMap map) =>
                {
                    if (_activeDownloadingID == mapID)
                    {
                        MapLoader.LoadMap(map);
                        DownloadManager.StopDownload();
                        LILogger.Notify("<color=#1a95d8>Download finished!</color>");
                        _activeDownloadingID = null;
                        //MapFileAPI.Instance.Save(map); // Maybe another time...
                    }
                }, (string error) => {
                    if (_activeDownloadingID == mapID)
                        DownloadManager.SetError(error);
                });
            }
        }
    }
}
