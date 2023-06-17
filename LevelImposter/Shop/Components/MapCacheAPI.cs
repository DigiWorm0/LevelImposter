using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to manage cached map asset bundles in the local filesystem
    /// </summary>
    public class MapCacheAPI : FileCache
    {
        public MapCacheAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        public static MapCacheAPI? Instance = null;

        private readonly JsonSerializerOptions SERIALIZE_OPTIONS = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Reads and parses a map file into a LIMap.
        /// </summary>
        /// <param name="mapID">Map ID to load</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public new LIMap? Get(string mapID)
        {
            if (!Exists(mapID))
            {
                LILogger.Warn($"Could not find map [{mapID}] in cache");
                return null;
            }

            LILogger.Info($"Loading map [{mapID}] from cache");

            string mapPath = GetPath(mapID);
            using (FileStream mapStream = File.OpenRead(mapPath))
            {
                LIMap? mapData = JsonSerializer.Deserialize<LIMap?>(mapStream);
                if (mapData != null)
                {
                    mapData.id = mapID;
                    return mapData;
                }
            }

            LILogger.Warn($"Failed to read map [{mapID}] from cache");
            return null;
        }

        /// <summary>
        /// Saves a map to the local map cache
        /// </summary>
        /// <param name="map">Map to save to cache</param>
        [HideFromIl2Cpp]
        public void Save(LIMap map)
        {
            LILogger.Info($"Saving {map} to filesystem");
            string mapJson = JsonSerializer.Serialize(map, SERIALIZE_OPTIONS);
            Save(map.id, mapJson);
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetFileExtension(".lim");
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}