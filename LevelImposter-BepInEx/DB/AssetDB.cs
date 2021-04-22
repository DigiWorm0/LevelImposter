using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LevelImposter.DB
{
    static class AssetDB
    {
        public static Dictionary<string, TaskData> tasks;
        public static Dictionary<string, UtilData> utils;
        public static Dictionary<string, SabData> sabs;
        public static Dictionary<string, DecData> dec;
        public static Dictionary<string, RoomData> room;
        public static Dictionary<string, SSData> ss;

        public static void Init()
        {
            try
            {
                TempDB tempDB = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDB>(
                    Encoding.UTF8.GetString(Properties.Resources.AssetDB, 0, Properties.Resources.AssetDB.Length)
                );

                tasks = tempDB.tasks;
                utils = tempDB.utils;
                sabs = tempDB.sabs;
                dec = tempDB.dec;
                room = tempDB.room;
                ss = tempDB.ss;
            }
            catch (Exception e)
            {
                LILogger.LogError("Error during AssetDB JSON Deserialization:\n" + e.Message);
            }
        }
        
        public static bool Import()
        {
            LILogger.LogInfo("...Loading Asset Database");
            var client = GameObject.Find("NetworkManager").GetComponent<AmongUsClient>();
            foreach (AssetReference assetRef in client.ShipPrefabs)
            {
                if (assetRef.IsDone)
                {
                    AssetDB.Import(assetRef.Asset.Cast<GameObject>());
                }
                else
                {
                    LILogger.LogError("There was an error loading the Asset Database!");
                    return false;
                }
            }
            return true;
        }

        private static void Import(GameObject map)
        {
            // Ship Status
            ShipStatus shipStatus = map.GetComponent<ShipStatus>();

            // Determine Map Type
            MapType mapType = MapType.Skeld;
            if (map.name == "AprilShip")
                return;
            if (map.name == "MiraShip")
                mapType = MapType.Mira;
            if (map.name == "PolusShip")
                mapType = MapType.Polus;
            if (map.name == "Airship")
                mapType = MapType.Airship;


            // Import Map to Lists
            Import(map, shipStatus, mapType, tasks);
            Import(map, shipStatus, mapType, utils);
            Import(map, shipStatus, mapType, sabs);
            Import(map, shipStatus, mapType, dec);
            Import(map, shipStatus, mapType, room);
            Import(map, shipStatus, mapType, ss);

            LILogger.LogInfo("..." + map.name + " Loaded");
        }

        private static void Import<T>(GameObject map, ShipStatus shipStatus, MapType mapType, Dictionary<string, T> list) where T : AssetData
        {
            foreach (var elem in list)
            {
                if (elem.Value.MapType == mapType)
                {
                    elem.Value.ImportMap(map, shipStatus);
                }
            }
        }
    }

    class TempDB
    {
        public Dictionary<string, TaskData> tasks;
        public Dictionary<string, UtilData> utils;
        public Dictionary<string, SabData> sabs;
        public Dictionary<string, DecData> dec;
        public Dictionary<string, RoomData> room;
        public Dictionary<string, SSData> ss;
    }
}
