using BepInEx.Logging;
using Il2CppSystem.Reflection;
using LevelImposter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Map
{
    static class MapHandler
    {
        public static MapData mapData;
        public static string  mapDir;
        public static bool    mapLoaded = false;

        public static bool Load()
        {
            string dllDir = System.Reflection.Assembly.GetAssembly(typeof( LevelImposter.Main )).Location;
            mapDir = Path.Combine(Path.GetDirectoryName(dllDir), "map.json");
            
            try
            {
                string mapJson = File.ReadAllText(mapDir);
                mapData = JsonConvert.DeserializeObject<MapData>(mapDir);
                mapLoaded = true;
                LILogger.LogInfo("Found and Deserialized Map Data");
                return true;
            }
            catch
            {
                mapData = new MapData();
                LILogger.LogError("Could not find map at " + mapDir);
            }
            return false;
        }

        public static MapData GetMap()
        {
            return mapData;
        }
    }
}
