using LevelImposter.Core;
using System;
using System.Collections.Generic;
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
        public static string SerializeMap(LIMap mapData)
        {
            // Map Data
            string mapJson = JsonSerializer.Serialize(mapData);

            // SpriteDB
            // TODO: Use stream instead of strings
            string rawSpriteDB = "";
            if (mapData.spriteDB != null)
            {
                foreach (KeyValuePair<Guid, SpriteDB.DBElement> sprite in mapData.spriteDB.DB)
                {
                    if (sprite.Value.rawData != null)
                        rawSpriteDB += $"{sprite.Key}={sprite.Value.rawData}\n";
                    else if (sprite.Value.fileChunk != null)
                        rawSpriteDB += $"{sprite.Key}={sprite.Value.fileChunk}\n";
                }
            }

            // Return
            // TODO: Use stream instead of strings
            return $"{mapJson}\n{rawSpriteDB}";
        }
    }
}
