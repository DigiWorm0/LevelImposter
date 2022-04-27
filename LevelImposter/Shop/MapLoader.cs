using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class MapLoader
    {
        public static LIMap LoadMap(string mapId)
        {
            string mapPath = GetPath(mapId);
            if (!File.Exists(mapPath))
            {
                LILogger.Info("Could not find map at " + mapPath);
                return null;
            }
            string mapJson = File.ReadAllText(mapPath);
            LIMap mapData = System.Text.Json.JsonSerializer.Deserialize<LIMap>(mapJson);
            LILogger.Msg("Loaded Map: " + mapData.name + " from " + mapPath);
            return mapData;
        }

        public static string GetPath(string mapId)
        {
            string dllDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter)).Location;
            return Path.Combine(Path.GetDirectoryName(dllDir), "LevelImposter", mapId + ".json");
        }
    }
}
