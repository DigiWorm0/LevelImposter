using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using System;
using System.IO;
using UnityEngine;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to manage thumbnail files in the local filesystem
    /// </summary>
    public static class ThumbnailCache
    {
        /// <summary>
        /// Checks if a thumbnail exists in the local cache
        /// </summary>
        /// <param name="mapID">ID of the map thumbnail to check</param>
        /// <returns><c>true</c> if the map thumbnail exists in the cache, <c>false</c> otherwise</returns>
        public static bool Exists(string mapID) => FileCache.Exists($"{mapID}.png");

        /// <summary>
        /// Saves a thumbnail to the local cache
        /// </summary>
        /// <param name="mapID">ID of the map thumbnail to save</param>
        /// <param name="thumbnailBytes">Raw image bytes to write to disk</param>
        public static void Save(string mapID, byte[] thumbnailBytes) => FileCache.Save($"{mapID}.png", thumbnailBytes);

        /// <summary>
        /// Reads and parses a thumbnail file into a Texture2D.
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public static void Get(string mapID, Action<Sprite?> callback)
        {
            if (!Exists(mapID))
            {
                LILogger.Warn($"Could not find [{mapID}] thumbnail in filesystem");
                return;
            }

            // Read thumbnail from filesystem
            LILogger.Info($"Loading thumbnail [{mapID}] from filesystem");
            bool isInSpriteCache = SpriteLoader.Instance?.IsSpriteInCache(mapID) ?? false;
            Stream? thumbnailStream = null;
            if (!isInSpriteCache)
                thumbnailStream = File.OpenRead(FileCache.GetPath($"{mapID}.png"));

            // Load thumbnail into sprite
            SpriteLoader.Instance?.LoadSpriteAsync(thumbnailStream, (spriteData) =>
            {
                Sprite? sprite = spriteData?.Sprite;
                if (sprite == null)
                {
                    LILogger.Warn($"Error loading [{mapID}] thumbnail from filesystem");
                    return;
                }
                callback.Invoke(spriteData?.Sprite);
            }, mapID, null);
        }
    }
}