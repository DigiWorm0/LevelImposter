using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using LevelImposter.Core;
using LevelImposter.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LevelImposter.DB
{
    class AssetDB
    {
        private static bool isLoaded = false;

        public static Dictionary<string, TaskData> tasks;
        public static Dictionary<string, UtilData> utils;
        public static Dictionary<string, SabData> sabs;
        public static Dictionary<string, DecData> dec;
        public static Dictionary<string, RoomData> room;
        public static Dictionary<string, SSData> ss;

        public static void Init()
        {
            TempDB tempDB = JsonSerializer.Deserialize<TempDB>(
                Encoding.UTF8.GetString(Properties.Resources.AssetDB, 0, Properties.Resources.AssetDB.Length)
            );

            tasks = tempDB.tasks;
            utils = tempDB.utils;
            sabs = tempDB.sabs;
            dec = tempDB.dec;
            room = tempDB.room;
            ss = tempDB.ss;
        }

        public static void Import()
        {
            if (isLoaded)
                return;
            isLoaded = true;
            LILogger.Info("Importing AssetDB...");
            AmongUsClient client = GameObject.Find("NetworkManager").GetComponent<AmongUsClient>();
            foreach (AssetReference prefab in client.ShipPrefabs)
                if (prefab.IsDone)
                    Import(prefab.Asset.Cast<GameObject>());
        }

        private static void Import(GameObject prefab)
        {
            ShipStatus shipStatus = prefab.GetComponent<ShipStatus>();
            MapType mapType = MapType.Skeld;
            if (prefab.name == "AprilShip")
                return;
            if (prefab.name == "MiraShip")
                mapType = MapType.Mira;
            if (prefab.name == "PolusShip")
                mapType = MapType.Polus;
            if (prefab.name == "Airship")
                mapType = MapType.Airship;

            Import(prefab, shipStatus, mapType, tasks);
            Import(prefab, shipStatus, mapType, utils);
            Import(prefab, shipStatus, mapType, sabs);
            Import(prefab, shipStatus, mapType, dec);
            Import(prefab, shipStatus, mapType, room);
            Import(prefab, shipStatus, mapType, ss);

            LILogger.Info("..." + prefab.name + " Loaded");
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
}