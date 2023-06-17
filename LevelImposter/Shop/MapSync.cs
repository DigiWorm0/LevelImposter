using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class MapSync
    {
        private static Guid? _activeDownloadingID = null;

        public static bool IsDownloadingMap => _activeDownloadingID != null;

        /// <summary>
        /// Regenerates the fallback ID and sets it as current map
        /// </summary>
        public static void RegenerateFallbackID()
        {
            bool isHost = AmongUsClient.Instance.AmHost;
            bool isPlayerInit = PlayerControl.LocalPlayer != null;
            bool isFreeplay = DestroyableSingleton<TutorialManager>.InstanceExists;
            if (!isHost || isFreeplay || !isPlayerInit)
                return;

            string? randomMapID = GetRandomMapID(new());
            if (randomMapID != null)
            {
                MapLoader.LoadMap(randomMapID, true, SyncMapID);
            }
            else
            {
                MapLoader.UnloadMap();
                SyncMapID();
            }
        }

        /// <summary>
        /// Syncs the map ID across all clients
        /// </summary>
        public static void SyncMapID()
        {
            bool isHost = AmongUsClient.Instance.AmHost;
            bool isPlayerInit = PlayerControl.LocalPlayer != null;
            bool isFreeplay = DestroyableSingleton<TutorialManager>.InstanceExists;
            if (!isHost || isFreeplay || !isPlayerInit)
                return;

            // Get ID
            string mapIDStr = MapLoader.CurrentMap?.id ?? Guid.Empty.ToString();
            if (!Guid.TryParse(mapIDStr, out _))
            {
                LILogger.Error($"Invalid map ID [{mapIDStr}]");
                return;
            }
            LILogger.Info($"[RPC] Transmitting map ID [{mapIDStr}] (fallback={MapLoader.IsFallback})");
            RPCSendMapID(PlayerControl.LocalPlayer, mapIDStr, MapLoader.IsFallback);

            // Set Map ID
            bool isEmpty = MapLoader.CurrentMap == null;
            if (!isEmpty && !MapLoader.IsFallback)
                MapUtils.SetLobbyMapType(MapType.LevelImposter);
            else if (isEmpty && GameOptionsManager.Instance.CurrentGameOptions.MapId == (byte)MapType.LevelImposter)
                MapUtils.SetLobbyMapType(MapType.Skeld);

        }

        [MethodRpc((uint)LIRpc.SyncMapID)]
        private static void RPCSendMapID(PlayerControl _, string mapIDStr, bool isFallback)
        {
            LILogger.Info($"[RPC] Received map ID [{mapIDStr}] (fallback={isFallback})");

            DownloadManager.Reset();
            if (DestroyableSingleton<GameStartManager>.InstanceExists)
                GameStartManager.Instance.ResetStartState();

            // Parse ID
            bool isSuccess = Guid.TryParse(mapIDStr, out Guid mapID);
            if (!isSuccess)
            {
                LILogger.Error($"Invalid map ID [{mapIDStr}]");
                return;
            }

            // Get Current
            string currentMapID = MapLoader.CurrentMap?.id ?? "";
            if (_activeDownloadingID != null && _activeDownloadingID != mapID)
            {
                LILogger.Notify("Download stopped.");
                _activeDownloadingID = null;
            }

            // Disable Fallback
            if (mapID.Equals(Guid.Empty))
            {
                MapLoader.UnloadMap();
            }
            // Currently Downloading
            else if (_activeDownloadingID == mapID)
            {
                DownloadManager.StartDownload();
            }
            // Already Loaded
            else if (currentMapID == mapIDStr)
            {
                MapLoader.SetFallback(isFallback);
                return;
            }
            // In Local Filesystem
            else if (MapFileAPI.Instance?.Exists(mapIDStr) == true)
            {
                MapLoader.LoadMap(mapIDStr, isFallback, null);
            }
            // In Local Cache
            else if (MapCacheAPI.Instance?.Exists(mapIDStr) == true)
            {
                MapLoader.LoadMap(MapCacheAPI.Instance?.Get(mapIDStr), isFallback);
            }
            // Download if Unavailable
            else
            {
                _activeDownloadingID = mapID;
                LILogger.Notify("<color=#1a95d8>Downloading map, please wait...</color>");
                MapLoader.UnloadMap();
                DownloadManager.StartDownload();
                LevelImposterAPI.DownloadMap(mapID, null, (LIMap map) =>
                {
                    MapCacheAPI.Instance?.Save(map);
                    if (_activeDownloadingID == mapID)
                    {
                        MapLoader.LoadMap(map, isFallback);
                        DownloadManager.StopDownload();
                        LILogger.Notify("<color=#1a95d8>Download finished!</color>");
                        _activeDownloadingID = null;
                        // TODO: Add map to local cache folder
                    }
                }, (string error) => {
                    if (_activeDownloadingID == mapID)
                        DownloadManager.SetError(error);
                    _activeDownloadingID = null;
                });
            }
        }

        private static string? GetRandomMapID(List<string> blacklistMaps)
        {
            if (MapFileAPI.Instance == null)
                throw new Exception("Missing MapFileAPI");

            // Get all custom maps
            var fileIDs = new List<string>(MapFileAPI.Instance.ListIDs());
            var mapIDs = fileIDs.FindAll(id => !blacklistMaps.Contains(id));
            if (mapIDs.Count <= 0)
            {
                LILogger.Warn("Map randomizer could not find any custom maps.");
                return null;
            }

            // Get map weights
            float[] mapWeights = new float[mapIDs.Count];
            float mapWeightSum = 0;
            for (int i = 0; i < mapIDs.Count; i++)
            {
                var mapWeight = ConfigAPI.GetMapWeight(mapIDs[i]);
                mapWeights[i] = mapWeightSum + mapWeight;
                mapWeightSum += mapWeight;
            }

            // Choose a random map
            float randomSum = UnityEngine.Random.Range(0, mapWeightSum);
            for (int i = 0; i < mapIDs.Count; i++)
            {
                string mapID = mapIDs[i];
                bool isOnline = Guid.TryParse(mapID, out _);
                if (mapWeights[i] >= randomSum)
                {
                    if (isOnline)
                    {
                        LILogger.Info($"Map randomizer chose [{mapID}]");
                        return mapID;
                    }
                    blacklistMaps.Add(mapID);
                    return GetRandomMapID(blacklistMaps);
                }
            }

            throw new Exception("Map randomizer reached an impossible state");
        }
    }
}
