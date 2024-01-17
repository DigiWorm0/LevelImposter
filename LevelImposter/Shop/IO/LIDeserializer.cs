using LevelImposter.Core;
using System;
using System.IO;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public static class LIDeserializer
    {
        public static LIMap? DeserializeMap(Stream dataStream, bool spriteDB = true, string? filePath = null)
        {
            try
            {
                // Parse Legacy
                byte firstByte = (byte)dataStream.ReadByte();
                dataStream.Position = dataStream.Length - 1;
                byte lastByte = (byte)dataStream.ReadByte();
                dataStream.Position = 0;
                string fileExtension = Path.GetExtension(filePath ?? "");

                bool isLegacy = firstByte == '{' && lastByte == '}' && fileExtension != "lim2";
                if (isLegacy)
                {
                    var legacyMap = JsonSerializer.Deserialize<LIMap>(dataStream);
                    if (legacyMap == null)
                        LILogger.Error("Failed to deserialize legacy map data");
                    else
                        LegacyConverter.UpdateMap(legacyMap);
                    return legacyMap;
                }

                // Map Data Length
                byte[] mapLengthBytes = new byte[4];
                dataStream.Read(mapLengthBytes, 0, 4);
                int mapLength = BitConverter.ToInt32(mapLengthBytes, 0);

                // Read Map Data
                byte[] mapDataBytes = new byte[mapLength];
                dataStream.Read(mapDataBytes, 0, mapLength);
                string mapDataString = System.Text.Encoding.UTF8.GetString(mapDataBytes);
                LIMap? mapData = JsonSerializer.Deserialize<LIMap>(mapDataString);

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
                    byte[] idBytes = new byte[36];
                    dataStream.Read(idBytes, 0, 36);
                    string idString = System.Text.Encoding.UTF8.GetString(idBytes);
                    bool isValidGUID = Guid.TryParse(idString, out Guid spriteID);
                    if (!isValidGUID)
                    {
                        LILogger.Error($"Failed to parse sprite ID: {idString}");
                        continue;
                    }

                    // Read Length
                    byte[] lengthBytes = new byte[4];
                    dataStream.Read(lengthBytes, 0, 4);
                    int dataLength = BitConverter.ToInt32(lengthBytes, 0);

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
                        var fileChunk = new FileChunk(filePath, dataStream.Position, dataLength);
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

                // Return
                return mapData;
            }
            catch (Exception e)
            {
                LILogger.Error(e.Message);
                return null;
            }
        }
    }
}
