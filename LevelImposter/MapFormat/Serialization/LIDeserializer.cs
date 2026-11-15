using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LevelImposter.Core.Utils;
using LevelImposter.MapFormat.Models;

namespace LevelImposter.MapFormat.Serialization;

/// <summary>
///     The detected on-disk format of a map file.
/// </summary>
public enum MapFileFormat
{
    Unknown,
    Legacy,
    LIM2,
    LIM2_Signature,
    LIM2_ZIP
}

/// <summary>
///     Structured result of a map parse attempt.
///     Used by the offline test suite and the in-game harness to distinguish
///     "invalid file" from "valid file with unsupported content".
/// </summary>
public sealed class MapLoadResult
{
    public MapFileFormat Format { get; init; } = MapFileFormat.Unknown;
    public bool Success { get; init; }
    public string? Error { get; init; }
    public Exception? Exception { get; init; }
    public string? MapID { get; init; }
    public int? MapVersion { get; init; }
    public int? ElementCount { get; init; }
    public int? AssetCount { get; init; }
}

/// <summary>
///     A set of options used for map deserialization.
/// </summary>
public sealed class MapLoadOptions
{
    /// <summary>
    ///     If true, the map's asset DB will be loaded into memory. Expensive for large maps, but required for some operations.
    /// </summary>
    public bool LoadAssetDB { get; init; } = false;

    /// <summary>
    ///     The file path of the map being loaded. Used for lazily loading map assets.
    /// </summary>
    public string FilePath { get; init; } = "";
}

public static class LIDeserializer
{
    /// <summary>
    ///     Given a data stream to an LIM file, this deserializes the file into an LIMap object.
    /// </summary>
    /// <param name="dataStream">The raw file data stream</param>
    /// <param name="options">Options for how to load the map</param>
    /// <returns>The deserialized LIMap object, or null if an error occurred</returns>
    public static LIMap? DeserializeMap(
        Stream dataStream,
        MapLoadOptions options)
    {
        if (TryDeserializeMap(dataStream, options, out var map, out var result))
            return map;

        LILogger.Error($"Error deserializing map data: {result.Error}");
        return null;
    }

    /// <summary>
    ///     Attempts to deserialize a map and returns structured diagnostics instead of swallowing the error.
    /// </summary>
    /// <param name="dataStream">The raw file data stream</param>
    /// <param name="options">Options for how to load the map</param>
    /// <param name="map">The deserialized map, or null on failure</param>
    /// <param name="result">Details about the detected format and any failure</param>
    /// <returns>True if the map was deserialized successfully</returns>
    public static bool TryDeserializeMap(
        Stream dataStream,
        MapLoadOptions options,
        out LIMap? map,
        out MapLoadResult result)
    {
        map = null;
        var format = MapFileFormat.Unknown;

        try
        {
            // Identify Format
            format = IdentifyFormat(dataStream);

            // Deserialize map
            map = format switch
            {
                MapFileFormat.Legacy => LIDeserializerLegacy.Deserialize(
                    dataStream,
                    options.FilePath),

                MapFileFormat.LIM2_ZIP => LIDeserializerZIP.Deserialize(
                    dataStream,
                    options.LoadAssetDB,
                    options.FilePath),

                MapFileFormat.LIM2 => LIDeserializerLIM.Deserialize(
                    dataStream,
                    options.LoadAssetDB,
                    options.FilePath),

                MapFileFormat.LIM2_Signature => LIDeserializerLIM.DeserializeWithSignature(
                    dataStream,
                    options.LoadAssetDB,
                    options.FilePath),

                _ => throw new InvalidDataException("Unknown map format")
            };

            // Migrate map
            MigrateMap(map);

            result = new MapLoadResult
            {
                Format = format,
                Success = true,
                MapID = map.id,
                MapVersion = map.v,
                ElementCount = map.elements.Length,
                AssetCount = map.MapAssetDB?.DB.Count
            };
            return true;
        }
        catch (Exception e)
        {
            result = new MapLoadResult
            {
                Format = format,
                Success = false,
                Error = e.Message,
                Exception = e
            };
            return false;
        }
    }

    /// <summary>
    ///     Identifies the map format from a data stream.
    /// </summary>
    /// <param name="dataStream">The raw file data stream</param>
    /// <returns>The identified map format</returns>
    /// <exception cref="InvalidDataException">Thrown if the stream cannot be read or seeked</exception>
    public static MapFileFormat IdentifyFormat(Stream dataStream)
    {
        if (!dataStream.CanSeek)
            throw new InvalidDataException("Map stream must be seekable to identify its format");

        // Check for LIM2 Signature
        var firstFourBytes = new byte[4];
        var bytesRead = dataStream.Read(firstFourBytes, 0, 4);
        if (bytesRead < 4)
            throw new InvalidDataException("Failed to read map format signature");

        dataStream.Position = 0;
        if (firstFourBytes[0] == 'L' &&
            firstFourBytes[1] == 'I' &&
            firstFourBytes[2] == 'M' &&
            firstFourBytes[3] == '2')
            return MapFileFormat.LIM2_Signature;

        // Check for ZIP Signature
        if (firstFourBytes[0] == 0x50 &&
            firstFourBytes[1] == 0x4B &&
            firstFourBytes[2] == 0x03 &&
            firstFourBytes[3] == 0x04)
            return MapFileFormat.LIM2_ZIP;

        // Check for Legacy
        dataStream.Position = dataStream.Length - 1;
        var lastByte = (byte)dataStream.ReadByte();
        dataStream.Position = 0;

        var isLegacy = firstFourBytes[0] == '{' &&
                       firstFourBytes[1] == '\"' &&
                       lastByte == '}';
        if (isLegacy)
            return MapFileFormat.Legacy;

        // Default to LIM2
        return MapFileFormat.LIM2;
    }

    private static void MigrateMap(LIMap map)
    {
        foreach (var element in map.elements)
        {
            // Fix Layer Transforms
            if (element.type == "util-layer" && map.v < 3)
            {
                element.x = 0;
                element.y = 0;
                element.z = 0;
                element.xScale = 1;
                element.yScale = 1;
                element.rotation = 0;
            }

            // Fix Trigger Fade
            if (element.properties.triggerFadeTime != null)
            {
                foreach (var trigger in element.properties.triggers ?? [])
                    trigger.Properties = new Dictionary<string, JsonElement>
                    {
                        { "fadeTime", JsonSerializer.SerializeToElement(element.properties.triggerFadeTime.Value) }
                    };

                element.properties.triggerFadeTime = null;
            }
        }
    }
}