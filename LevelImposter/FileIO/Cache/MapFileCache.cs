using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using LevelImposter.Networking.API;
using System;
using System.IO;

namespace LevelImposter.FileIO;

/// <summary>
/// API to manage cached map asset bundles in the local filesystem
/// </summary>
public static class MapFileCache
{
    /// <summary>
    /// Checks if a map exists in the local map cache
    /// </summary>
    /// <param name="mapID">ID of the map to check</param>
    /// <returns><c>true</c> if the map exists in the cache, <c>false</c> otherwise</returns>
    public static bool Exists(string mapID) => FileCache.Exists($"{mapID}.lim2");

    /// <summary>
    /// Gets the path to a map file in the local map cache
    /// </summary>
    /// <param name="mapID">ID of the map to find</param>
    /// <returns>The path to the map file</returns>
    public static string GetPath(string mapID) => FileCache.GetPath($"{mapID}.lim2");

    /// <summary>
    /// Reads and parses a map file into a LIMap.
    /// </summary>
    /// <param name="mapID">Map ID to load</param>
    /// <param name="callback">Callback on success</param>
    [HideFromIl2Cpp]
    public static LIMap? Get(string mapID)
    {
        // Check if map exists
        if (!Exists(mapID))
        {
            LILogger.Warn($"Could not find map [{mapID}] in cache");
            return null;
        }

        LILogger.Info($"Loading map [{mapID}] from cache");

        // Deserialize map file
        var mapPath = GetPath(mapID);
        using var fileStream = File.OpenRead(mapPath);
        var mapData = LIDeserializer.DeserializeMap(fileStream, true, mapPath);
        
        // Reassign map ID (just in case)
        if (mapData != null)
        {
            mapData.id = mapID;
            return mapData;
        }

        LILogger.Warn($"Failed to read map [{mapID}] from cache");
        return null;
    }
    
    /// <summary>
    /// Downloads a specific map from the LevelImposter API and saves it to the local cache.
    /// </summary>
    /// <param name="id">ID of the map to download</param>
    /// <param name="onProgress">Callback on download progress</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void DownloadMap(
        Guid id,
        Action<float>? onProgress,
        Action<FileStore> onSuccess,
        Action<string>? onError = null)
    {
        LevelImposterAPI.DownloadMap(
            id,
            GetPath(id.ToString()),
            onProgress,
            onSuccess,
            onError
        );
    }
}
