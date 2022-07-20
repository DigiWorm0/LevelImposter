using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class MapLoader
    {
        public static LIMap currentMap = null;

        public static void WriteMap(Guid mapID, string mapData)
        {
            LILogger.Info("Writing map [" + mapID + "] to filesystem");
            string mapPath = GetPath(mapID);
            string mapDir = Path.GetDirectoryName(mapPath);
            if (!Directory.Exists(mapDir)) {
                Directory.CreateDirectory(mapDir);
            }
            File.WriteAllText(mapPath, mapData);
        }

        public static void DeleteMap(Guid mapID)
        {
            if (!Exists(mapID))
            {
                LILogger.Error("Could not find map [" + mapID + "] in filesystem");
                return;
            }
            LILogger.Info("Removing map [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            File.Delete(mapPath);
        }
        public static void LoadMap(Guid mapID)
        {
            if (!Exists(mapID))
            {
                LILogger.Error("Could not find [" + mapID + "] in filesystem");
                return;
            }
            LILogger.Info("Loading map [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            string mapJson = File.ReadAllText(mapPath);
            currentMap = System.Text.Json.JsonSerializer.Deserialize<LIMap>(mapJson);
        }

        public static void UnloadMap()
        {
            currentMap = null;
        }

        public static bool Exists(Guid mapId)
        {
            string mapPath = GetPath(mapId);
            return File.Exists(mapPath);
        }

        public static string GetPath(Guid mapId)
        {
            string dllDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter)).Location;
            return Path.Combine(Path.GetDirectoryName(dllDir), "LevelImposter", mapId + ".lim");
        }
    }
}
