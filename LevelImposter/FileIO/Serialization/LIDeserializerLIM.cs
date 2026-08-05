using System;
using System.IO;
using System.Text;
using System.Text.Json;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.FileIO.DataStores;
using LevelImposter.FileIO.Streams;

namespace LevelImposter.FileIO.Serialization;

public class LIDeserializerLIM
{
    public static LIMap DeserializeWithSignature(
        Stream dataStream,
        bool spriteDB = true,
        string? filePath = null
    )
    {
        dataStream.Position += 4;
        return Deserialize(dataStream, spriteDB, filePath);
    }

    public static LIMap Deserialize(
        Stream dataStream,
        bool spriteDB = true,
        string? filePath = null
    )
    {
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
        mapData.MapAssetDB = new MapAssetDB();
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
            if (dataLength <= 0 ||
                dataStream.Position + dataLength > dataStream.Length)
            {
                LILogger.Error($"Invalid data length: {dataLength}");
                continue;
            }

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
                // Reading from a stream, save the raw data to memory
                mapData.MapAssetDB.Add(spriteID, dataStream.ToIl2CppArray(dataLength));
            }
        }

        return mapData;
    }
}