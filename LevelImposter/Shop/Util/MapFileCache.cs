using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Shop
{
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
        /// Saves raw map data to the local map cache
        /// </summary>
        /// <param name="memoryBlock">Raw map data to save</param>
        /// <param name="mapID">ID of the map to save</param>
        [HideFromIl2Cpp]
        public static void Save(MemoryBlock memoryBlock, string mapID)
        {
            LILogger.Info($"Saving {mapID} to filesystem");
            FileCache.Save($"{mapID}.lim2", memoryBlock);
        }
    }
}