using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Core;

namespace LevelImposter.Shop;

/// <summary>
///     Converts LIM to LIM2 files
/// </summary>
public static class LegacyConverter
{
    /// <summary>
    ///     Gets the legacy path for a map
    /// </summary>
    /// <param name="mapID">ID of the map to get</param>
    /// <returns>The path of the legacy map file</returns>
    public static string GetLegacyPath(string mapID)
    {
        return Path.Combine(MapFileAPI.GetDirectory(), $"{mapID}.lim");
    }

    /// <summary>
    ///     Auto-Updates all downloaded maps
    /// </summary>
    /// <returns></returns>
    [HideFromIl2Cpp]
    public static IEnumerator ConvertAllMaps()
    {
        {
            var legacyMapIDs = Directory.GetFiles(MapFileAPI.GetDirectory(), "*.lim");
            foreach (var legacyMapID in legacyMapIDs)
            {
                var mapID = Path.GetFileNameWithoutExtension(legacyMapID);
                if (!MapFileAPI.Exists(mapID))
                {
                    ShopManager.Instance?.SetOverlayEnabled(true);
                    ShopManager.Instance?.SetOverlayText($"Converting legacy maps...\n<size=2>{mapID}");
                    yield return null;
                    ConvertFile(mapID);
                }
            }

            ShopManager.Instance?.SetOverlayEnabled(false);
        }
    }

    /// <summary>
    ///     Compares two byte arrays (Il2CppStructArray<byte>)
    /// </summary>
    /// <param name="data1">The first byte array</param>
    /// <param name="data2">The second byte array</param>
    /// <returns>True if the byte arrays match, false otherwise.</returns>
    private static bool CompareData(Il2CppStructArray<byte>? data1, Il2CppStructArray<byte>? data2)
    {
        if (data1 == null || data2 == null)
            return false;
        if (data1.Length != data2.Length)
            return false;
        for (var i = 0; i < data1.Length; i++)
            if (data1[i] != data2[i])
                return false;
        return true;
    }

    /// <summary>
    ///     Finds asset data in the assetDB or adds it if it doesn't exist
    /// </summary>
    /// <param name="assetDB">AssetDB to search or add</param>
    /// <param name="data">Data to search for or add</param>
    /// <returns>The resulting asset ID</returns>
    private static Guid FindOrAddAsset(MapAssetDB assetDB, Il2CppStructArray<byte> data)
    {
        // Find Asset
        foreach (var asset in assetDB.DB)
            if (CompareData(asset.Value.LoadToMemory().Data, data))
                return asset.Key;

        // Create Asset
        var assetID = Guid.NewGuid();
        assetDB.Add(assetID, new MemoryBlock(data));
        return assetID;
    }

#pragma warning disable CS0618 // Handles legacy properties
    /// <summary>
    ///     Updates legacy map data to a LIM2 data
    /// </summary>
    /// <param name="map">Legacy Map Data</param>
    public static void UpdateMap(LIMap map)
    {
        if (!map.isLegacy)
            return;

        LILogger.Info($"Converting legacy map data [{map.id}]");

        // Update Properties
        map.isLegacy = false;
        map.mapAssetDB = new MapAssetDB();

        // SpriteDB
        foreach (var element in map.elements)
        {
            // Add Sprite Data
            if (element.properties.spriteData != null)
            {
                var spriteData = MapUtils.ParseBase64(element.properties.spriteData);
                element.properties.spriteID = FindOrAddAsset(map.mapAssetDB, spriteData);
                element.properties.spriteData = null;
            }

            // Add Meeting Background
            if (element.properties.meetingBackground != null)
            {
                var spriteData = MapUtils.ParseBase64(element.properties.meetingBackground);
                element.properties.meetingBackgroundID = FindOrAddAsset(map.mapAssetDB, spriteData);
                element.properties.spriteData = null;
            }

            // Add Minigame Data
            if (element.properties.minigames != null)
                foreach (var minigame in element.properties.minigames)
                {
                    var spriteData = MapUtils.ParseBase64(minigame.spriteData ?? "");
                    if (spriteData != null)
                        minigame.spriteID = FindOrAddAsset(map.mapAssetDB, spriteData);
                    minigame.spriteData = null;
                }

            // Add Sound Data
            if (element.properties.sounds != null)
                foreach (var sound in element.properties.sounds)
                {
                    if (sound.isPreset)
                    {
                        sound.presetID = sound.data;
                    }
                    else
                    {
                        var soundData = MapUtils.ParseBase64(sound.data ?? "");
                        if (soundData != null)
                            sound.dataID = FindOrAddAsset(map.mapAssetDB, soundData);
                    }

                    sound.data = null;
                }

            // TODO: Search for duplicate entries
        }
    }
#pragma warning restore CS0618

    /// <summary>
    ///     Converts a legacy map file to a LIM2 file
    /// </summary>
    /// <param name="mapID">ID of the map to convert</param>
    /// <exception cref="FileNotFoundException">If the map file wasn't found</exception>
    /// <exception cref="FileLoadException">If the new map already exists</exception>
    public static void ConvertFile(string mapID)
    {
        LILogger.Info($"Converting legacy map file [{mapID}]");

        // Get paths
        var legacyPath = GetLegacyPath(mapID);
        var newPath = MapFileAPI.GetPath(mapID);

        // Check if files exist
        if (!File.Exists(legacyPath))
            throw new FileNotFoundException($"Could not find legacy map file {legacyPath}");
        if (File.Exists(newPath))
            throw new FileLoadException($"Map file {newPath} already exists");

        // Read legacy file
        LIMap? mapFile;
        using (var legacyFileStream = File.OpenRead(legacyPath))
        {
            mapFile = JsonSerializer.Deserialize<LIMap>(legacyFileStream);
        }

        // Check if file is valid
        if (mapFile == null)
            throw new FileLoadException($"Could not deserialize legacy map file {legacyPath}");

        // Update map
        UpdateMap(mapFile);

        // Serialize & Write to new file
        using var outputFileStream = File.Create(newPath);
        LISerializer.SerializeMap(mapFile, outputFileStream);

        // Delete legacy file
        File.Move(legacyPath, $"{legacyPath}.bak");
    }
}