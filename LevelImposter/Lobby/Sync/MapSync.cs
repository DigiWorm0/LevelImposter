using System;
using LevelImposter.Core;
using LevelImposter.FileIO;
using LevelImposter.Networking;
using Reactor.Networking.Rpc;

namespace LevelImposter.Lobby;

public static class MapSync
{
    private static Guid? _activeDownloadingID;

    public static bool IsDownloadingMap => _activeDownloadingID != null;

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
        var mapIDStr = GameConfiguration.CurrentMap?.id ?? Guid.Empty.ToString();
        if (!Guid.TryParse(mapIDStr, out var mapID))
        {
            LILogger.Error($"Invalid map ID [{mapIDStr}]");
            return;
        }

        // Transmit RPC
        Rpc<MapSyncRPC>.Instance.Send(PlayerControl.LocalPlayer, new MapState
        {
            MapID = mapID,
            RandomizerSeed = RandomizerSync.GenerateRandomSeed(),
            IsMapNameHidden = GameConfiguration.HideMapName
        });
    }

    public static void OnRPCSyncMapID(Guid mapID, bool isHidden)
    {
        var mapIDStr = mapID.ToString();
        DownloadManager.Reset();
        
        // Cancel Game Start
        if (DestroyableSingleton<GameStartManager>.InstanceExists)
            GameStartManager.Instance.ResetStartState();

        // Get Current
        var currentMapID = GameConfiguration.CurrentMap?.id ?? "";
        if (_activeDownloadingID != null && _activeDownloadingID != mapID)
        {
            LILogger.Notify("Download stopped.", false);
            _activeDownloadingID = null;
        }

        // No Map Selected
        if (mapID.Equals(Guid.Empty))
        {
            GameConfiguration.SetMap(null);
            return;
        }

        // Currently Downloading
        if (_activeDownloadingID == mapID)
        {
            DownloadManager.StartDownload();
        }
        // Already Loaded
        else if (currentMapID == mapIDStr)
        {
            GameConfiguration.SetMap(GameConfiguration.CurrentMap, isHidden);
        }
        // In Local Filesystem
        else if (MapFileAPI.Exists(mapIDStr))
        {
            var map = MapFileAPI.Get(mapIDStr);
            if (map == null)
                throw new Exception($"Map file [{mapIDStr}] could not be loaded from filesystem");
            
            GameConfiguration.SetMap(map, isHidden);
        }
        // In Local Cache
        else if (MapFileCache.Exists(mapIDStr))
        {
            var map = MapFileCache.Get(mapIDStr);
            if (map == null)
                throw new Exception($"Map file [{mapIDStr}] could not be loaded from cache");
            
            GameConfiguration.SetMap(map, isHidden);
        }
        // Download to cache if unavailable
        else
        {
            _activeDownloadingID = mapID;
            LILogger.Notify("Downloading map, please wait...", false);
            GameConfiguration.SetMap(null);
            DownloadManager.StartDownload();
            MapFileCache.DownloadMap(
                mapID,
                DownloadManager.SetProgress,
                (_) =>
                {
                    if (_activeDownloadingID != mapID)
                        return;
                    
                    var map = MapFileCache.Get(mapIDStr);
                    if (map == null)
                        throw new Exception($"Map file [{mapIDStr}] could not be loaded from cache after download");
                    
                    GameConfiguration.SetMap(map, isHidden);
                    
                    // Finish Download
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

    
    
}