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
        public static string checksum = "";
        public static bool mapLoaded = false;

        public static bool Load()
        {
            if (mapLoaded)
                return true;

            LILogger.LogInfo("...Deserializing Map Data");

            // Get Directory
            string dllDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter.MainHarmony)).Location;
            mapDir = Path.Combine(Path.GetDirectoryName(dllDir), "map.json");

            // Load File
            if (!File.Exists(mapDir))
            {
                LILogger.LogError("Could not find map at " + mapDir);
                return false;
            }
            string mapJson = File.ReadAllText(mapDir);

            // Settings
            var settings = new Newtonsoft.Json.JsonSerializerSettings { 
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore
            };

            // Deserialize
            try
            {
                mapData = Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(mapJson, settings);
            }
            catch (Exception e)
            {
                LILogger.LogError("There was an error deserializing map data:\n" + e.Message);
                return false;
            }

            // Checksum
            int checkNum = 0;
            foreach (char c in mapJson)
            {
                checkNum += Convert.ToByte(c);
            }
            checksum = checkNum.ToString("X4");

            // Return
            mapLoaded = true;
            return true;
        }

        public static MapData GetMap()
        {
            return mapData;
        }

        public static MapAsset GetById(long id)
        {
            return mapData.objs.FirstOrDefault(obj => obj.id == id);
        }
    }
}
