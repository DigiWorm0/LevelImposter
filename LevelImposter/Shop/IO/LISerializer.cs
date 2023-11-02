using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public static class LISerializer
    {
        private static JsonSerializerOptions? _options = null;

        /// <summary>
        /// Serializes a map into a string
        /// </summary>
        /// <param name="mapData">Map Data to serialize</param>
        /// <returns>Raw LIM2 file data</returns>
        public static MemoryStream SerializeMap(LIMap mapData)
        {
            // Create Options
            if (_options == null)
            {
                _options = new();
                _options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            }

            // Open Stream
            MemoryStream stream = new();

            // Update Legacy Format
            if (mapData.isLegacy)
                LegacyConverter.UpdateMap(mapData);

            // Map Data
            byte[] mapJsonBytes = JsonSerializer.SerializeToUtf8Bytes(mapData, _options);
            stream.Write(BitConverter.GetBytes(mapJsonBytes.Length));
            stream.Write(mapJsonBytes);

            // SpriteDB
            if (mapData.mapAssetDB != null)
            {
                foreach (KeyValuePair<Guid, MapAssetDB.DBElement> sprite in mapData.mapAssetDB.DB)
                {
                    var data = sprite.Value.ToBytes();

                    // Write Element
                    stream.Write(sprite.Key.ToByteArray());
                    stream.Write(BitConverter.GetBytes(data.Length));
                    stream.Write(data);
                }
            }

            // Return
            stream.Position = 0;
            return stream;
        }
    }
}
