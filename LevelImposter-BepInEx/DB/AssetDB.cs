using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    static class AssetDB
    {
        public static Dictionary<string, TaskData> tasks;

        public static void Init()
        {
            try
            {
                TempDB tempDB = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDB>(
                    Encoding.UTF8.GetString(Properties.Resources.AssetDB, 0, Properties.Resources.AssetDB.Length)
                );

                tasks = tempDB.tasks;
            }
            catch
            {
                LILogger.LogError("Error during AssetDB JSON Deserialization");
            }
        }

        public static void ImportMap(GameObject map)
        {
            // Ship Status
            ShipStatus shipStatus = map.GetComponent<ShipStatus>();

            // Determine Map Type
            ShipStatus.MapType mapType = ShipStatus.MapType.Pb;
            if (map.name == "AprilShip")
                return;
            if (map.name == "MiraShip")
                mapType = ShipStatus.MapType.Hq;
            if (map.name == "SkeldShip")
                mapType = ShipStatus.MapType.Ship;
            

            // Import Map to Lists
            ImportMap(map, shipStatus, mapType, tasks);
        }

        private static void ImportMap<T>(GameObject map, ShipStatus shipStatus, ShipStatus.MapType mapType, Dictionary<string, T> list) where T : AssetData
        {
            foreach (var elem in list)
            {
                if (elem.Value.MapType == mapType)
                    elem.Value.ImportMap(map, shipStatus);
            }
        }
    }

    class TempDB
    {
        public Dictionary<string, TaskData> tasks;
    }
}
