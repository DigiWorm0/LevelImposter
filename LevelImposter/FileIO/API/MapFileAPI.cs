using System;
using System.IO;
using System.Linq;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.FileIO.DataStores;
using LevelImposter.FileIO.Serialization;
using LevelImposter.Networking.API;

namespace LevelImposter.FileIO.API;

/// <summary>
///     API to manage LIM files in the local filesystem
/// </summary>
public static class MapFileAPI
{
    public static readonly MapFileCache AllMaps = new();

    /// <summary>
    ///     Gets the current directory where LevelImposter map files are stored.
    ///     Usually in a LevelImposter folder beside the LevelImposter.dll.
    /// </summary>
    /// <returns>String path where LevelImposter data is stored.</returns>
    public static string GetDirectory()
    {
        return FileAPI.GetPath();
    }

    /// <summary>
    ///     Attempts to open the LevelImposter maps folder in whatever file explorer the device has.
    /// </summary>
    public static void OpenInExplorer()
    {
        FileAPI.OpenInExplorer();
    }

    /// <summary>
    ///     Deserializes all map files in the local filesystem.
    /// </summary>
    /// <returns>A list of all map files in the local filesystem.</returns>
    public static MapFileInfo[] GetAllMaps()
    {
        return Directory.GetFiles(GetDirectory(), "*.lim2")
            .Select(ReadMetadataFromPath)
            .OfType<MapFileInfo>()
            .ToArray();
    }

    private static MapFileInfo? ReadMetadataFromPath(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            return LIDeserializer.DeserializeMap(stream, false, filePath);
        }
        catch (Exception e)
        {
            LILogger.Warn($"Failed to read map file [{filePath}]: {e}");
            return null;
        }
    }


    /// <summary>
    ///     Reads the full map data from a map file located in the LevelImposter folder.
    /// </summary>
    /// <param name="metadata">Metadata of the map file</param>
    /// <returns>Map data of the map file</returns>
    /// <exception cref="ArgumentNullException">Thrown if the metadata is null</exception>
    public static MapFile? GetMapData(MapFileInfo? metadata)
    {
        if (metadata?.FilePath == null)
            throw new ArgumentNullException(
                $"Metadata object does not have a file path. Cannot load map data for {metadata?.ID}");

        using var stream = File.OpenRead(metadata.FilePath);
        return LIDeserializer.DeserializeMap(stream, true, metadata.FilePath);
    }

    /// <summary>
    ///     Deletes a map file from the filesystem.
    /// </summary>
    /// <param name="map">The map file to delete</param>
    public static void Delete(MapFileInfo map)
    {
        if (map.FilePath == null)
            throw new ArgumentNullException(
                $"Map {map} does not have a file path, cannot delete");

        LILogger.Info($"Deleting {map} from filesystem");
        if (File.Exists(map.FilePath))
            File.Delete(map.FilePath);
    }

    /// <summary>
    ///     Initializes the LevelImposter folder in the local filesystem.
    /// </summary>
    public static void Init()
    {
        if (!Directory.Exists(GetDirectory()))
            Directory.CreateDirectory(GetDirectory());
    }

    /// <summary>
    ///     Downloads a specific map from the LevelImposter API and saves it to the local filesystem.
    /// </summary>
    /// <param name="mapInfo">Info about the map file</param>
    /// <param name="onProgress">Callback on download progress</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void DownloadMap(
        MapFileInfo mapInfo,
        Action<float>? onProgress,
        Action<FileStore> onSuccess,
        Action<string>? onError = null)
    {
        if (!mapInfo.IsInWorkshop)
            throw new ArgumentException($"Map {mapInfo} is not a workshop map, cannot download");

        Init();
        LevelImposterAPI.DownloadMap(
            new Guid(mapInfo.ID),
            GetDownloadPath(mapInfo),
            onProgress,
            onSuccess,
            onError
        );
    }

    private static string GetDownloadPath(MapFileInfo mapInfo)
    {
        var iteration = 0;
        var sanitizedName = SafeFileAPI.Sanitize(mapInfo.Name);
        var path = Path.Combine(GetDirectory(), $"{sanitizedName}.lim2");
        while (File.Exists(path))
        {
            if (iteration > 100)
                throw new InvalidOperationException($"Failed to find a valid file path for map {mapInfo}");

            iteration++;
            path = Path.Combine(GetDirectory(), $"{sanitizedName} {iteration}.lim2");
        }

        return path;
    }

    public class MapFileCache
    {
        private MapFileInfo[] _cachedMaps = [];

        /// <summary>
        ///     Gets the cached list of all maps in the local filesystem.
        /// </summary>
        /// <returns>A list of all map files in the local filesystem.</returns>
        public MapFileInfo[] Get()
        {
            return _cachedMaps;
        }

        /// <summary>
        ///     Refreshes the list of all maps
        /// </summary>
        public void Refresh()
        {
            _cachedMaps = GetAllMaps();
        }

        /// <summary>
        ///     Tries to find and deserialize a map file by its ID.
        /// </summary>
        /// <param name="mapID">ID of the map file</param>
        /// <param name="mapInfo">The deserialized map info object</param>
        /// <returns>Metadata of the map file</returns>
        public bool TryGetByID(string? mapID, out MapFileInfo? mapInfo)
        {
            mapInfo = null;
            if (string.IsNullOrEmpty(mapID))
                return false;

            mapInfo = _cachedMaps.FirstOrDefault(m => m.ID == mapID);
            return mapInfo != null;
        }
    }
}