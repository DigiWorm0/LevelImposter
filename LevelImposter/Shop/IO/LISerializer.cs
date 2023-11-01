using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public static class LISerializer
    {
        /// <summary>
        /// Serializes a map into a string
        /// </summary>
        /// <param name="mapData">Map Data to serialize</param>
        /// <returns>Raw LIM2 file data</returns>
        public static MemoryStream SerializeMap(LIMap mapData)
        {
            MemoryStream stream = new();

            // Map Data
            byte[] mapJsonBytes = JsonSerializer.SerializeToUtf8Bytes(mapData);
            stream.Write(BitConverter.GetBytes(mapJsonBytes.Length));
            stream.Write(mapJsonBytes);

            // SpriteDB
            if (mapData.spriteDB != null)
            {
                foreach (KeyValuePair<Guid, SpriteDB.DBElement> sprite in mapData.spriteDB.DB)
                {
                    var data = sprite.Value.ToBytes();

                    // Write ID
                    stream.Write(sprite.Key.ToByteArray());

                    // Write Length
                    stream.Write(BitConverter.GetBytes(data.Length));

                    // Write Value
                    stream.Write(data);
                }
            }

            // Return
            stream.Position = 0;
            return stream;
        }
    }
}
