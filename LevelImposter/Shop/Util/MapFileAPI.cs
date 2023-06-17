using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LevelImposter.Core;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to manage LIM files in the local filesystem
    /// </summary>
    public static class MapFileAPI
    {
        private static readonly JsonSerializerOptions SERIALIZE_OPTIONS = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Gets the current directory where LevelImposter map files are stored.
        /// Usually in a LevelImposter folder beside the LevelImposter.dll.
        /// </summary>
        /// <returns>String path where LevelImposter data is stored.</returns>
        public static string GetDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
            return Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter");
        }

        /// <summary>
        /// Gets the path where a specific map LIM file is stored.
        /// </summary>
        /// <param name="mapID">ID of the map file</param>
        /// <returns>The path where a specific map is stored</returns>
        public static string GetPath(string mapID)
        {
            return Path.Combine(GetDirectory(), mapID + ".lim");
        }

        /// <summary>
        /// Lists all map file IDs that are located in the LevelImposter folder.
        /// </summary>
        /// <returns>Array of map file IDs that are located in the LevelImpsoter folder.</returns>
        [HideFromIl2Cpp]
        public static string[] ListIDs()
        {
            string[] fileNames = Directory.GetFiles(GetDirectory(), "*.lim");
            for (int i = 0; i < fileNames.Length; i++)
                fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
            return fileNames;
        }

        /// <summary>
        /// Checks the existance of a map file based on ID
        /// </summary>
        /// <param name="mapID">Map File ID</param>
        /// <returns>True if a map file with the cooresponding ID exists</returns>
        public static bool Exists(string? mapID)
        {
            if (mapID == null)
                return false;
            return File.Exists(GetPath(mapID));
        }

        /// <summary>
        /// Reads and parses a map file into memory.
        /// </summary>
        /// <param name="mapID">Map ID to read and parse</param>
        /// <param name="callback">Callback on success</param>
        /// <returns>Representation of the map file data in the form of a <c>LIMap</c>.</returns>
        [HideFromIl2Cpp]
        public static void Get(string mapID, Action<LIMap?> callback)
        {
            string path = GetPath(mapID);
            FileHandler.Instance?.Get(path, callback, null);
        }

        /// <summary>
        /// Reads and parses the metadata of a map file into memory.
        /// Less memory-intensive than <c>MapFileAPI.Get()</c>.
        /// </summary>
        /// <param name="mapID">Map ID to read and parse</param>
        /// <param name="callback">Callback on success</param>
        /// <returns>Representation of the map file data in the form of a <c>LIMetadata</c>.</returns>
        [HideFromIl2Cpp]
        public static void GetMetadata(string mapID, Action<LIMetadata?> callback)
        {
            // TODO: Prevent parsing of unnecessary data
            string path = GetPath(mapID);
            FileHandler.Instance?.Get(path, callback, null);
        }

        /// <summary>
        /// Saves map data into the local filesystem based on the map's ID.
        /// </summary>
        /// <param name="map">Map data to save</param>
        [HideFromIl2Cpp]
        public static void Save(LIMap map)
        {
            LILogger.Info($"Saving {map} to filesystem");
            string mapPath = GetPath(map.id);
            string mapJson = JsonSerializer.Serialize(map, SERIALIZE_OPTIONS);
            if (!Directory.Exists(GetDirectory()))
                Directory.CreateDirectory(GetDirectory());
            File.WriteAllText(mapPath, mapJson);
        }

        /// <summary>
        /// Deletes a map file from the filesystem.
        /// </summary>
        /// <param name="mapID">ID of the map to delete</param>
        public static void Delete(string mapID)
        {
            LILogger.Info($"Deleting [{mapID}] from filesystem");
            string mapPath = GetPath(mapID);
            File.Delete(mapPath);
        }

        /// <summary>
        /// Initializes the LevelImposter folder in the local filesystem.
        /// </summary>
        public static void Init()
        {
            if (!Directory.Exists(GetDirectory()))
                Directory.CreateDirectory(GetDirectory());
        }
    }
}