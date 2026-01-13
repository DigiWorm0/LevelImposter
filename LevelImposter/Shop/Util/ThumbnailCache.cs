using System;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Shop;

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
    ///     Saves a thumbnail to the local cache
    /// </summary>
    /// <param name="mapID">ID of the map thumbnail to save</param>
    /// <param name="thumbnailBytes">Raw image bytes to write to disk</param>
    public static void Save(string mapID, MemoryBlock thumbnailBytes)
    {
        FileCache.Save($"{mapID}.png", thumbnailBytes);
    }

    /// <summary>
    ///     Reads and parses a thumbnail file into a Texture2D.
    /// </summary>
    /// <param name="mapID">Map ID for the thumbnail</param>
    /// <param name="callback">Callback on success</param>
    [HideFromIl2Cpp]
    public static void Get(string mapID, Action<Sprite> callback)
    {
        if (!Exists(mapID))
        {
            LILogger.Warn($"Could not find [{mapID}] thumbnail in filesystem");
            return;
        }

        // Read thumbnail from filesystem
        LILogger.Info($"Loading thumbnail [{mapID}] from filesystem");

        // Load thumbnail into sprite
        SpriteLoader.LoadAsync(
            $"{mapID}_thumb",
            new FileStore(FileCache.GetPath($"{mapID}.png")),
            callback
        );
    }
}