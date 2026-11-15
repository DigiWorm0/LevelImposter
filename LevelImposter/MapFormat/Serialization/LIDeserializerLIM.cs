using System;
using System.IO;
using System.Text;
using System.Text.Json;
using LevelImposter.MapFormat.Models;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.MapFormat.Serialization;

public class LIDeserializerLIM
{
    public static LIMap DeserializeWithSignature(
        Stream dataStream,
        bool spriteDB = true,
        string? filePath = null)
    {
        // Validate the LIM2 signature instead of blindly skipping it
        var signature = new byte[4];
        dataStream.ReadExactly(signature, signature.Length);
        if (signature[0] != 'L' ||
            signature[1] != 'I' ||
            signature[2] != 'M' ||
            signature[3] != '2')
            throw new Exception("Invalid LIM2 signature");

        return Deserialize(dataStream, spriteDB, filePath);
    }

    public static LIMap Deserialize(
        Stream dataStream,
        bool spriteDB = true,
        string? filePath = null)
    {
        // Map Data Length
        var mapLengthBytes = new byte[4];
        dataStream.ReadExactly(mapLengthBytes, mapLengthBytes.Length);
        var mapLength = BitConverter.ToInt32(mapLengthBytes, 0);
        if (mapLength <= 0 || mapLength > dataStream.Length - dataStream.Position)
            throw new Exception($"Invalid map data length: {mapLength}");

        // Read Map Data
        var mapDataBytes = new byte[mapLength];
        dataStream.ReadExactly(mapDataBytes, mapLength);
        var mapDataString = Encoding.UTF8.GetString(mapDataBytes);
        var mapData = JsonSerializer.Deserialize<LIMap>(mapDataString);

        // Check Map Data
        if (mapData == null)
            throw new Exception("Failed to deserialize map data");

        // Abort if no SpriteDB
        if (!spriteDB)
            return mapData;

        // Read SpriteDB
        mapData.MapAssetDB = new MapAssetDB();
        while (dataStream.Position < dataStream.Length)
        {
            // Read ID
            var idBytes = new byte[36];
            dataStream.ReadExactly(idBytes, idBytes.Length);
            var idString = Encoding.UTF8.GetString(idBytes);
            var isValidGUID = Guid.TryParse(idString, out var spriteID);
            if (!isValidGUID)
                throw new Exception($"Failed to parse sprite ID: {idString}");

            // Read Length
            var lengthBytes = new byte[4];
            dataStream.ReadExactly(lengthBytes, lengthBytes.Length);
            var dataLength = BitConverter.ToInt32(lengthBytes, 0);

            // Check Length
            if (dataLength <= 0 ||
                dataStream.Position + dataLength > dataStream.Length)
                throw new Exception($"Invalid data length: {dataLength}");

            // Read Data
            if (filePath != null)
            {
                // Reading from a file, just save the File Stream offset
                var fileChunk = new FileChunkStore(filePath, dataStream.Position, dataLength);
                mapData.MapAssetDB.Add(spriteID, fileChunk);
                dataStream.Position += dataLength;
            }
            else
            {
                // Reading from a stream, save the raw data to managed memory
                mapData.MapAssetDB.Add(spriteID, new MemoryStore(dataStream.ReadBytes(dataLength)));
            }
        }

        return mapData;
    }
}