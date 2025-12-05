using System;
using System.IO;
using System.Text;
using System.Text.Json;
using LevelImposter.Core;

namespace LevelImposter.Shop;

public static class LIDeserializer
{
    private const float JS_MAX_SAFE_INTEGER = 9007199254740991;

    private enum MapFormat
    {
        Legacy,
        LIM2,
        LIM2_SIGNATURE,
        LIM2_ZIP
    }

    public static LIMap? DeserializeMap(Stream dataStream, bool spriteDB = true, string? filePath = null)
    {
        try
        {
            // Identify Format
            var mapFormat = IdentifyMapFormat(dataStream, filePath);
            LILogger.Info($"Identified map format: {mapFormat}");
            
            // Legacy Map
            if (mapFormat == MapFormat.Legacy)
            {
                var legacyMap = JsonSerializer.Deserialize<LIMap>(dataStream);
                if (legacyMap == null)
                    LILogger.Error("Failed to deserialize legacy map data");
                else
                    LegacyConverter.UpdateMap(legacyMap);
                return legacyMap;
            }
            
            // Decompress ZIP
            if (mapFormat == MapFormat.LIM2_ZIP)
                return LICompressedDeserializer.Deserialize(dataStream, spriteDB, filePath);
            
            // LIM2 Signature Map
            if (mapFormat == MapFormat.LIM2_SIGNATURE)
                dataStream.Position += 4;

            // Map Data Length
            var mapLengthBytes = new byte[4];
            dataStream.Read(mapLengthBytes, 0, 4);
            var mapLength = BitConverter.ToInt32(mapLengthBytes, 0);

            // Read Map Data
            var mapDataBytes = new byte[mapLength];
            dataStream.Read(mapDataBytes, 0, mapLength);
            var mapDataString = Encoding.UTF8.GetString(mapDataBytes);
            var mapData = JsonSerializer.Deserialize<LIMap>(mapDataString);

            // Check Map Data
            if (mapData == null)
                throw new Exception("Failed to deserialize map data");

            // Abort if no SpriteDB
            if (!spriteDB)
                return mapData;

            // Read SpriteDB
            mapData.mapAssetDB = new MapAssetDB();
            while (dataStream.Position < dataStream.Length)
            {
                // Read ID
                var idBytes = new byte[36];
                dataStream.Read(idBytes, 0, 36);
                var idString = Encoding.UTF8.GetString(idBytes);
                var isValidGUID = Guid.TryParse(idString, out var spriteID);
                if (!isValidGUID)
                {
                    LILogger.Error($"Failed to parse sprite ID: {idString}");
                    continue;
                }

                // Read Length
                var lengthBytes = new byte[4];
                dataStream.Read(lengthBytes, 0, 4);
                var dataLength = BitConverter.ToInt32(lengthBytes, 0);

                // Check Length
                if (dataLength <= 0)
                {
                    LILogger.Error($"Invalid data length: {dataLength}");
                    continue;
                }

                // Read Data
                if (filePath != null)
                {
                    // Reading from a file, just save the File Stream offset
                    var fileChunk = new FileChunkStore(filePath, dataStream.Position, dataLength);
                    mapData.mapAssetDB.Add(spriteID, fileChunk);
                    dataStream.Position += dataLength;
                }
                else
                {
                    // Reading from a stream, save the raw data to memory
                    var buffer = new byte[dataLength];
                    dataStream.Read(buffer, 0, dataLength);
                    mapData.mapAssetDB.Add(spriteID, buffer);
                }
            }

            // Migrate
            MigrateMap(mapData);

            // Return
            return mapData;
        }
        catch (Exception e)
        {
            LILogger.Error(e.Message);
            return null;
        }
    }

    /// <summary>
    /// Identifies the map format from a data stream
    /// </summary>
    /// <param name="dataStream">The raw file data stream</param>
    /// <param name="filePath">Optional file path for extension checking</param>
    /// <returns>The identified map format</returns>
    /// <exception cref="Exception">Thrown if the signature cannot be read</exception>
    private static MapFormat IdentifyMapFormat(Stream dataStream, string? filePath = null)
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
        var firstByte = (byte)dataStream.ReadByte();
        dataStream.Position = dataStream.Length - 1;
        var lastByte = (byte)dataStream.ReadByte();
        dataStream.Position = 0;
        
        var fileExtension = Path.GetExtension(filePath ?? "");
        var isLegacy = firstByte == '{' && lastByte == '}' && fileExtension != "lim2";
        if (isLegacy)
            return MapFormat.Legacy;
        
        // Default to LIM2
        return MapFormat.LIM2;
    }

    private static void MigrateMap(LIMap map)
    {
        // TODO: Add advanced map migration logic
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
}