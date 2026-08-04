using System;
using System.IO;
using System.Text.Json;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.FileIO.DataBlock;

namespace LevelImposter.FileIO.Serialization;

/// <summary>
///     Converts LIM to LIM2 files
/// </summary>
public static class LegacyConverter
{
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
        if (!map.IsLegacy)
            return;

        LILogger.Info($"Converting legacy map data [{map.id}]");

        // Update Properties
        map.IsLegacy = false;
        map.MapAssetDB = new MapAssetDB();

        // SpriteDB
        foreach (var element in map.elements)
        {
            // Add Sprite Data
            if (element.properties.spriteData != null)
            {
                var spriteData = ParseBase64(element.properties.spriteData);
                element.properties.spriteID = FindOrAddAsset(map.MapAssetDB, spriteData);
                element.properties.spriteData = null;
            }

            // Add Meeting Background
            if (element.properties.meetingBackground != null)
            {
                var spriteData = ParseBase64(element.properties.meetingBackground);
                element.properties.meetingBackgroundID = FindOrAddAsset(map.MapAssetDB, spriteData);
                element.properties.spriteData = null;
            }

            // Add Minigame Data
            if (element.properties.minigames != null)
                foreach (var minigame in element.properties.minigames)
                {
                    var spriteData = ParseBase64(minigame.spriteData ?? "");
                    if (spriteData != null)
                        minigame.spriteID = FindOrAddAsset(map.MapAssetDB, spriteData);
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
                        var soundData = ParseBase64(sound.data ?? "");
                        if (soundData != null)
                            sound.dataID = FindOrAddAsset(map.MapAssetDB, soundData);
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
    /// <param name="dataStream">The stream of map data to read from</param>
    /// <param name="filePath">
    ///     Optional file path to the map file.
    ///     If provided, this will replace the legacy map file with a new LIM2 file.
    /// </param>
    /// <exception cref="FileNotFoundException">If the map file wasn't found</exception>
    /// <exception cref="FileLoadException">If the new map already exists</exception>
    public static LIMap ConvertFile(
        Stream dataStream,
        string? filePath = null)
    {
        // Get paths
        var legacyPath = filePath;
        var newPath = Path.ChangeExtension(filePath, ".lim2");
        LILogger.Info($"Converting legacy map file @ {filePath} ({newPath})");

        // Read legacy file
        var mapFile = JsonSerializer.Deserialize<LIMap>(dataStream);
        if (mapFile == null)
            throw new FileLoadException($"Could not deserialize legacy map file @ {filePath}");

        // Update map
        UpdateMap(mapFile);

        // Legacy >>> .bak
        var backupPath = $"{filePath}.bak";
        const int index = 0;
        while (File.Exists(backupPath))
            backupPath = $"{filePath}.bak.{index}";

        dataStream.Close();
        if (File.Exists(legacyPath))
            File.Move(legacyPath, backupPath);

        // Updated >>> .lim2
        if (File.Exists(newPath))
            File.Delete(newPath);

        if (newPath != null)
        {
            using var outputFileStream = File.Create(newPath);
            LISerializer.SerializeMap(mapFile, outputFileStream);
        }

        return mapFile;
    }

    private static byte[] ParseBase64(string base64)
    {
        var sub64 = base64.Substring(base64.IndexOf(",", StringComparison.Ordinal) + 1);
        return Convert.FromBase64String(sub64);
    }
}