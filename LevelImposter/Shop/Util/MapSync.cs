using System;
using System.Collections.Generic;
using System.IO;
using LevelImposter.Core;
using LevelImposter.FileIO;
using LevelImposter.Networking;
using LevelImposter.Networking.API;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Random = UnityEngine.Random;

namespace LevelImposter.Shop;

public static class MapSync
{
    private static Guid? _activeDownloadingID;

    public static bool IsDownloadingMap => _activeDownloadingID != null;

    /// <summary>
    ///     Regenerates the fallback ID and sets it as current map
    /// </summary>
    public static void RegenerateFallbackID()
    {
        if (!GameState.IsPlayerLoaded ||
            !GameState.IsHost ||
            GameState.IsInFreeplay)
            return;

        var randomMapID = GetRandomMapID(new List<string>());
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
    ///     Syncs the map ID across all clients
    /// </summary>
    public static void SyncMapID()
    {
        if (!GameState.IsHost ||
            !PlayerControl.LocalPlayer ||
            GameState.IsInFreeplay)
            return;

        // Get ID
        var mapIDStr = MapLoader.CurrentMap?.id ?? Guid.Empty.ToString();
        if (!Guid.TryParse(mapIDStr, out var mapID))
        {
            LILogger.Error($"Invalid map ID [{mapIDStr}]");
            return;
        }

        LILogger.Info($"[RPC] Transmitting map ID [{mapIDStr}] (fallback={MapLoader.IsFallback})");
        Rpc<MapSyncRPC>.Instance.Send(PlayerControl.LocalPlayer, new MapState
        {
            MapID = mapID,
            RandomizerSeed = RandomizerSync.GenerateRandomSeed(),
            IsFallback = MapLoader.IsFallback
        });

        // Set Map ID
        if (GameState.IsCustomMapLoaded && !MapLoader.IsFallback)
            MapUtils.SetLobbyMapType(MapType.LevelImposter);
        else if (!GameState.IsCustomMapLoaded && GameState.IsCustomMapSelected)
            MapUtils.SetLobbyMapType(MapType.Skeld, true);
    }

    public static void OnRPCSyncMapID(Guid mapID, bool isFallback)
    {
        LILogger.Info($"[RPC] Received map ID [{mapID.ToString()}] (fallback={isFallback})");

        var mapIDStr = mapID.ToString();
        DownloadManager.Reset();
        if (DestroyableSingleton<GameStartManager>.InstanceExists)
            GameStartManager.Instance.ResetStartState();

        // Get Current
        var currentMapID = MapLoader.CurrentMap?.id ?? "";
        if (_activeDownloadingID != null && _activeDownloadingID != mapID)
        {
            LILogger.Notify("Download stopped.", false);
            _activeDownloadingID = null;
        }

        // Disable Fallback
        if (mapID.Equals(Guid.Empty))
        {
            MapLoader.UnloadMap();
            return;
        }

        // Set Map Type
        if (!isFallback)
            MapUtils.SetLobbyMapType(MapType.LevelImposter);

        // Currently Downloading
        if (_activeDownloadingID == mapID)
        {
            DownloadManager.StartDownload();
        }
        // Already Loaded
        else if (currentMapID == mapIDStr)
        {
            MapLoader.SetFallback(isFallback);
        }
        // In Local Filesystem
        else if (MapFileAPI.Exists(mapIDStr))
        {
            MapLoader.LoadMap(mapIDStr, isFallback, null);
        }
        // In Local Cache
        else if (MapFileCache.Exists(mapIDStr))
        {
            MapLoader.LoadMap(MapFileCache.Get(mapIDStr), isFallback);
        }
        // Download to cache if unavailable
        else
        {
            _activeDownloadingID = mapID;
            LILogger.Notify("Downloading map, please wait...", false);
            MapLoader.UnloadMap();
            DownloadManager.StartDownload();
            MapFileCache.DownloadMap(
                mapID,
                DownloadManager.SetProgress,
                (_) =>
                {
                    if (_activeDownloadingID != mapID)
                        return;
                    
                    MapLoader.LoadMap(MapFileCache.Get(mapIDStr), isFallback);
                    DownloadManager.StopDownload();
                    LILogger.Notify("Download finished", false);
                    _activeDownloadingID = null;
                },
                error =>
                {
                    if (_activeDownloadingID == mapID)
                        DownloadManager.SetError(error);
                    _activeDownloadingID = null;
                });
        }
    }

    private static string? GetRandomMapID(List<string> blacklistMaps)
    {
        // Get all custom maps
        var fileIDs = new List<string>(MapFileAPI.ListIDs());
        var mapIDs = fileIDs.FindAll(id => !blacklistMaps.Contains(id));
        if (mapIDs.Count <= 0)
        {
            LILogger.Warn("Map randomizer could not find any custom maps.");
            return null;
        }

        // Get map weights
        var mapWeights = new float[mapIDs.Count];
        float mapWeightSum = 0;
        for (var i = 0; i < mapIDs.Count; i++)
        {
            var mapWeight = ConfigAPI.GetMapWeight(mapIDs[i]);
            mapWeights[i] = mapWeightSum + mapWeight;
            mapWeightSum += mapWeight;
        }

        // Choose a random map
        var randomSum = Random.Range(0, mapWeightSum);
        for (var i = 0; i < mapIDs.Count; i++)
        {
            var mapID = mapIDs[i];
            var isOnline = Guid.TryParse(mapID, out _);
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