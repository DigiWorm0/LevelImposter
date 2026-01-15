using System;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using LevelImposter.Networking.API;
using UnityEngine;

namespace LevelImposter.FileIO;

/// <summary>
///     API to manage thumbnail files in the local filesystem
/// </summary>
public static class ThumbnailCache
{
    /// <summary>
    ///     Checks if a thumbnail exists in the local cache
    /// </summary>
    /// <param name="mapID">ID of the map thumbnail to check</param>
    /// <returns><c>true</c> if the map thumbnail exists in the cache, <c>false</c> otherwise</returns>
    public static bool Exists(string mapID)
    {
        return FileCache.Exists($"{mapID}.png");
    }
    
    /// <summary>
    ///     Gets the file path for a thumbnail in the local cache
    /// </summary>
    /// <param name="mapID">Map ID for the thumbnail</param>
    /// <returns>String path to the thumbnail file</returns>
    public static string GetPath(string mapID)
    {
        return FileCache.GetPath($"{mapID}.png");
    }

    /// <summary>
    ///     Reads and parses a thumbnail file into a Texture2D.
    /// </summary>
    /// <param name="mapID">Map ID for the thumbnail</param>
    /// <param name="callback">Callback on success</param>
    public static void Get(string mapID, Action<Sprite> callback)
    {
        // Check if mapID is a valid GUID
        if (!Guid.TryParse(mapID, out _))
            return;
        
        // Check if thumbnail exists
        if (!Exists(mapID))
        {
            DownloadThumbnail(mapID, callback);
            return;
        }

        // Read thumbnail from filesystem
        LILogger.Info($"Loading thumbnail for [{mapID}] from filesystem");

        // Load thumbnail into sprite
        SpriteLoader.LoadAsync(
            $"{mapID}_thumb",
            new FileStore(GetPath(mapID)),
            callback
        );
    }

    /// <summary>
    /// Downloads and caches a thumbnail from the LevelImposter API
    /// </summary>
    /// <param name="mapID">Map ID for the thumbnail</param>
    /// <param name="callback">Callback on success</param>
    private static void DownloadThumbnail(string mapID, Action<Sprite> callback)
    {
        LILogger.Info($"Downloading thumbnail for [{mapID}]");
        LevelImposterAPI.DownloadThumbnail(mapID, GetPath(mapID), callback);
    }
}