using LevelImposter.Core;
using System;
using System.IO;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public static class LIDeserializer
    {
        public static string? CurrentFilePath = null;

        public static LIMap DeserializeMap(Stream dataStream, bool spriteDB = true)
        {
            // Map Data Length
            byte[] mapLengthBytes = new byte[4];
            dataStream.Read(mapLengthBytes, 0, 4);
            int mapLength = BitConverter.ToInt32(mapLengthBytes, 0);

            // Read Map Data
            byte[] mapDataBytes = new byte[mapLength];
            dataStream.Read(mapDataBytes, 0, mapLength);
            LIMap? mapData = JsonSerializer.Deserialize<LIMap>(mapDataBytes);

            // Check Map Data
            if (mapData == null)
                throw new Exception("Failed to deserialize map data");

            // Abort if no SpriteDB
            if (!spriteDB)
                return mapData;

            // Read SpriteDB
            mapData.mapAssetDB = new();
            while (dataStream.Position < dataStream.Length)
            {
                // Read ID
                byte[] idBytes = new byte[16];
                dataStream.Read(idBytes, 0, 16);
                Guid spriteID = new(idBytes);

                // Read Length
                byte[] lengthBytes = new byte[4];
                dataStream.Read(lengthBytes, 0, 4);
                int dataLength = BitConverter.ToInt32(lengthBytes, 0);

                // Save Chunk
                mapData.mapAssetDB.DB[spriteID] = new MapAssetDB.DBElement()
                {
                    fileChunk = new FileChunk(CurrentFilePath ?? "", dataStream.Position, dataLength)
                };
                dataStream.Position += dataLength;
            }

            // Return
            return mapData;
        }
    }
}
