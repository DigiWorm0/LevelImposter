using BepInEx.Logging;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    static class MapHandler
    {
        public static MapData mapData;
        public static string mapDir;
        public static bool mapLoaded = false;

        public static bool Load()
        {
            string dllDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter.MainHarmony)).Location;
            mapDir = Path.Combine(Path.GetDirectoryName(dllDir), "map.json");

            // Load File
            if (!File.Exists(mapDir))
            {
                LILogger.LogError("Could not find map at " + mapDir);
                return false;
            }
            string mapJson = File.ReadAllText(mapDir);

            // Deserialize
            mapData = Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(mapJson);

            mapLoaded = true;
            LILogger.LogInfo("Found and Deserialized Map Data");
            return true;
        }

        public static MapData GetMap()
        {
            return mapData;
        }
    }
}
