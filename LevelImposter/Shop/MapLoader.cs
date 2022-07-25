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

        public static void WriteMap(string mapID, string mapData)
        {
            LILogger.Info("Writing map [" + mapID + "] to filesystem");
            string mapPath = GetPath(mapID);
            string mapDir = Path.GetDirectoryName(mapPath);
            if (!Directory.Exists(mapDir)) {
                Directory.CreateDirectory(mapDir);
            }
            File.WriteAllText(mapPath, mapData);
        }

        public static void DeleteMap(string mapID)
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
        public static LIMap GetMap(string mapID)
        {
            if (!Exists(mapID))
            {
                LILogger.Error("Could not find [" + mapID + "] in filesystem");
                return null;
            }
            LILogger.Info("Loading map [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            string mapJson = File.ReadAllText(mapPath);
            LIMap mapData = System.Text.Json.JsonSerializer.Deserialize<LIMap>(mapJson);
            return mapData;
        }

        public static void LoadMap(string mapID)
        {
            currentMap = GetMap(mapID);
        }
        public static void UnloadMap()
        {
            currentMap = null;
        }

        public static bool Exists(string mapId)
        {
            string mapPath = GetPath(mapId);
            return File.Exists(mapPath);
        }

        public static string GetDir()
        {
            string dllDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter)).Location;
            return Path.Combine(Path.GetDirectoryName(dllDir), "LevelImposter");
        }

        public static string GetPath(string mapId)
        {
            return Path.Combine(GetDir(), mapId + ".lim");
        }

        public static string[] GetMapIDs()
        {
            string[] paths = Directory.GetFiles(GetDir());
            for (int i = 0; i < paths.Length; i++)
                paths[i] = Path.GetFileNameWithoutExtension(paths[i]);
            return paths;
        }
    }
}
