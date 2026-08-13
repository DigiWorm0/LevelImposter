using System;
using System.IO;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;

namespace LevelImposter.FileIO.Serialization;

public static class LIDeserializer
{
    /// <summary>
    ///     Given a data stream to an LIM file, this deserializes the file into an LIMap object.
    /// </summary>
    /// <param name="dataStream">The raw file data stream</param>
    /// <param name="loadAssetDB">If true, the map's assetDB will also be deserialized (expensive operation)</param>
    /// <param name="filePath">The file path of the map file, used to avoid loading asset data into memory when possible</param>
    /// <returns>The deserialized LIMap object, or null if an error occurred</returns>
    public static LIMap? DeserializeMap(
        Stream dataStream,
        bool loadAssetDB = true,
        string? filePath = null
    )
    {
        try
        {
            // Identify Format
            var mapFormat = IdentifyMapFormat(dataStream);

            // Deserialize map
            var map = mapFormat switch
            {
                MapFormat.Legacy => LIDeserializerLegacy.Deserialize(dataStream, filePath),
                MapFormat.LIM2_ZIP => LIDeserializerZIP.Deserialize(dataStream, loadAssetDB, filePath),
                MapFormat.LIM2 => LIDeserializerLIM.Deserialize(dataStream, loadAssetDB, filePath),
                _ => LIDeserializerLIM.DeserializeWithSignature(
                    dataStream,
                    loadAssetDB,
                    filePath)
            };

            // Migrate map
            MigrateMap(map);

            return map;
        }
        catch (Exception e)
        {
            LILogger.Error($"Error deserializing map data: {e.Message}");
            return null;
        }
    }

    /// <summary>
    ///     Identifies the map format from a data stream
    /// </summary>
    /// <param name="dataStream">The raw file data stream</param>
    /// <returns>The identified map format</returns>
    /// <exception cref="Exception">Thrown if the signature cannot be read</exception>
    private static MapFormat IdentifyMapFormat(Stream dataStream)
    {
        // Check for LIM2 Signature
        var firstFourBytes = new byte[4];
        var bytesRead = dataStream.Read(firstFourBytes, 0, 4);
        if (bytesRead < 4)
            throw new Exception("Failed to read map format signature");

        dataStream.Position = 0;
        if (firstFourBytes[0] == 'L' &&
            firstFourBytes[1] == 'I' &&
            firstFourBytes[2] == 'M' &&
            firstFourBytes[3] == '2')
            return MapFormat.LIM2_SIGNATURE;

        // Check for ZIP Signature
        if (firstFourBytes[0] == 0x50 &&
            firstFourBytes[1] == 0x4B &&
            firstFourBytes[2] == 0x03 &&
            firstFourBytes[3] == 0x04)
            return MapFormat.LIM2_ZIP;

        // Check for Legacy
        dataStream.Position = dataStream.Length - 1;
        var lastByte = (byte)dataStream.ReadByte();
        dataStream.Position = 0;

        var isLegacy = firstFourBytes[0] == '{' &&
                       firstFourBytes[1] == '\"' &&
                       lastByte == '}';
        if (isLegacy)
            return MapFormat.Legacy;

        // Default to LIM2
        return MapFormat.LIM2;
    }

    private static void MigrateMap(LIMap map)
    {
        // Fix Layer Transforms
        foreach (var element in map.elements)
            if (element.type == "util-layer" && map.v < 3)
            {
                element.x = 0;
                element.y = 0;
                element.z = 0;
                element.xScale = 1;
                element.yScale = 1;
                element.rotation = 0;
            }
    }

    private enum MapFormat
    {
        Legacy,
        LIM2,
        LIM2_SIGNATURE,
        LIM2_ZIP
    }
}