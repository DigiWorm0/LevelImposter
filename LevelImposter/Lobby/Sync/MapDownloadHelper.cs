using System;
using LevelImposter.Core;
using LevelImposter.FileIO;

namespace LevelImposter.Lobby;

/// <summary>
/// Manages downloading maps from the server to the client.
/// </summary>
public class MapDownloadHelper(bool preventGameStart = false)
{
    /// The current download state, or null if no download is active.
    public DownloadState? CurrentDownloadState { get; private set; }
    
    /// <summary>
    /// Checks if the specified map is currently being downloaded.
    /// </summary>
    /// <param name="mapID">ID of the map to check</param>
    /// <returns>True if the map is being downloaded, false otherwise</returns>
    public bool IsDownloadingMap(Guid mapID)
    {
        return CurrentDownloadState != null &&
               CurrentDownloadState.MapID == mapID;
    }

    /// <summary>
    ///   Cancels the active map download, if any.
    /// </summary>
    public void CancelDownload()
    {
        if (CurrentDownloadState == null)
            return;
        
        LILogger.Notify("Download cancelled.", false);
        CurrentDownloadState = null;

        // Allow Game Start
        if (preventGameStart)
            PlayersReadyCounter.MarkPlayerReady(PlayerControl.LocalPlayer);
    }

    /// <summary>
    /// Downloads a map with the specified ID.
    /// </summary>
    /// <param name="mapID">ID of the map to download</param>
    /// <param name="onSuccess">Callback invoked on successful download with the downloaded map</param>
    /// <param name="onError">Callback invoked on download error with the error message</param>
    public void DownloadMap(
        Guid mapID,
        Action<LIMap> onSuccess,
        Action<string> onError)
    {
        // Abort if already downloading this map
        if (CurrentDownloadState != null && 
            CurrentDownloadState.MapID == mapID)
            return;

        // Cancel any existing download
        CancelDownload();
        
        // Update state
        var downloadState = new DownloadState
{
            MapID = mapID
        };
        CurrentDownloadState = downloadState;

        // Prevent Game Start
        if (preventGameStart)
            PlayersReadyCounter.MarkPlayerNotReady(PlayerControl.LocalPlayer);
        
        // Log
        LILogger.Info($"Starting download for map ID [{mapID}]");
        LILogger.Notify("Downloading map, please wait...", false);

        // DownloadManager.StartDownload();
        MapFileCache.DownloadMap(
            mapID,
            progress => downloadState.Progress = progress,
            (_) =>
            {
                // Check if we changed downloads
                if (CurrentDownloadState?.ID != downloadState.ID)
                    return;
                
                // Load map from cache
                var map = MapFileCache.Get(mapID.ToString());
                if (map == null) {
                    downloadState.Error = "Failed to load downloaded map from cache.";
                    LILogger.Error(downloadState.Error);
                    onError(downloadState.Error);
                    return;
                }

                // Log
                LILogger.Info($"Successfully downloaded {map}");
                LILogger.Notify("Download finished", false);

                // Success
                onSuccess(map);
                
                // Allow Game Start
                if (preventGameStart)
                    PlayersReadyCounter.MarkPlayerReady(PlayerControl.LocalPlayer);

                // Clear state
                CurrentDownloadState = null;

            },
            error =>
            {
                // Check if we changed downloads
                if (CurrentDownloadState?.ID != downloadState.ID)
                    return;

                // Handle Error
                downloadState.Error = error;
                LILogger.Error($"Map download failed: {error}");
                onError(downloadState.Error);
            });
    }

    /// <summary>
    ///   Represents the state of an active map download.
    /// </summary>
    public class DownloadState
    {
        private static int _downloadIDCounter;

        public int ID = _downloadIDCounter++;
        public float Progress = 0;
        public Guid MapID = Guid.Empty;
        public string? Error;
    }
}