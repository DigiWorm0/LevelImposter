using LevelImposter.Core;
using System;
using System.IO;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public static class LIDeserializer
    {
        public static LIMap DeserializeMap(Stream dataStream, bool spriteDB = true)
        {
            // Open Reader
            using StreamReader reader = new(dataStream);

            // Read Map Data
            string? mapJson = reader.ReadLine();
            LIMap? mapData = JsonSerializer.Deserialize<LIMap>(mapJson ?? "");

            // Check Map Data
            if (mapData == null)
                throw new Exception("Failed to deserialize map data");

            // Abort if no SpriteDB
            if (!spriteDB)
                return mapData;

            // Read SpriteDB
            mapData.spriteDB = new();
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (line == null)
                    continue;

                int splitIndex = line.IndexOf('=');
                if (splitIndex == -1)
                    continue;

                // Read Key/Value
                string key = line.Substring(0, splitIndex);
                string value = line.Substring(splitIndex + 1);

                // Parse
                Guid.TryParse(key, out Guid spriteID);
                mapData.spriteDB.DB[spriteID] = new SpriteDB.DBElement()
                {
                    rawData = value // TODO: Use FileChunk instead of rawData
                };
            }

            // Return
            return mapData;
        }
    }
}
