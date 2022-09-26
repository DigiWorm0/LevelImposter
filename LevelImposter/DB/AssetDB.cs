using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using LevelImposter.Core;
using System.Text.Json;
using System.Collections;
using BepInEx.IL2CPP.Utils.Collections;

namespace LevelImposter.DB
{
    class AssetDB : MonoBehaviour
    {
        public static AssetDB Instance { get; private set; }

        public static bool isReady = false;

        public static Dictionary<string, TaskData> tasks;
        public static Dictionary<string, UtilData> utils;
        public static Dictionary<string, SabData> sabs;
        public static Dictionary<string, DecData> dec;
        public static Dictionary<string, RoomData> room;
        public static Dictionary<string, SSData> ss;
        public static Dictionary<string, SoundData> sounds;

        public void Start()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            TempDB tempDB = JsonSerializer.Deserialize<TempDB>(
                Encoding.UTF8.GetString(Properties.Resources.AssetDB, 0, Properties.Resources.AssetDB.Length)
            );

            tasks = tempDB.tasks;
            utils = tempDB.utils;
            sabs = tempDB.sabs;
            dec = tempDB.dec;
            room = tempDB.room;
            ss = tempDB.ss;
            sounds = tempDB.sounds;

            StartCoroutine(CoLoadAssets().WrapToIl2Cpp());
        }

        public IEnumerator CoLoadAssets()
        {
            LILogger.Info("Loading AssetDB...");
            foreach (AssetReference shipRef in AmongUsClient.Instance.ShipPrefabs)
            {
                yield return shipRef.LoadAssetAsync<GameObject>();
                GameObject shipPrefab = shipRef.Asset.Cast<GameObject>();
                yield return ImportAsset(shipPrefab);
            }
            isReady = true;
        }

        public IEnumerator ImportAsset(GameObject prefab)
        {
            ShipStatus shipStatus = prefab.GetComponent<ShipStatus>();
            MapType mapType = MapType.Skeld;
            if (prefab.name == "AprilShip")
                yield break;
            if (prefab.name == "MiraShip")
                mapType = MapType.Mira;
            if (prefab.name == "PolusShip")
                mapType = MapType.Polus;
            if (prefab.name == "Airship")
                mapType = MapType.Airship;

            yield return Import(prefab, shipStatus, mapType, tasks);
            yield return Import(prefab, shipStatus, mapType, utils);
            yield return Import(prefab, shipStatus, mapType, sabs);
            yield return Import(prefab, shipStatus, mapType, dec);
            yield return Import(prefab, shipStatus, mapType, room);
            yield return Import(prefab, shipStatus, mapType, ss);
            yield return Import(prefab, shipStatus, mapType, sounds);

            LILogger.Info("..." + prefab.name + " Loaded");
        }

        private IEnumerator Import<T>(GameObject map, ShipStatus shipStatus, MapType mapType, Dictionary<string, T> list) where T : AssetData
        {
            foreach (var elem in list)
            {
                if (elem.Value.MapType == mapType)
                {
                    elem.Value.ImportMap(map, shipStatus);
                    yield return null;
                }
            }
        }
    }
}