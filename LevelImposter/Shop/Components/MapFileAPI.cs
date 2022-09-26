using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using BepInEx.IL2CPP.Utils.Collections;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public class MapFileAPI : MonoBehaviour
    {
        public static MapFileAPI Instance;

        public MapFileAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public string GetDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter)).Location;
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(gameDir), "LevelImposter");
        }

        public string GetPath(string mapID)
        {
            return System.IO.Path.Combine(GetDirectory(), mapID + ".lim");
        }

        public string[] ListIDs()
        {
            string[] fileNames = System.IO.Directory.GetFiles(GetDirectory(), "*.lim");
            for (int i = 0; i < fileNames.Length; i++)
                fileNames[i] = System.IO.Path.GetFileNameWithoutExtension(fileNames[i]);
            return fileNames;
        }

        public bool Exists(string mapID)
        {
            return System.IO.File.Exists(GetPath(mapID));
        }

        public LIMap Get(string mapID)
        {
            if (!Exists(mapID))
            {
                LILogger.Error("Could not find [" + mapID + "] in filesystem");
                return null;
            }
            LILogger.Info("Loading map [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            string mapJson = File.ReadAllText(mapPath);
            LIMap mapData = JsonSerializer.Deserialize<LIMap>(mapJson);
            mapData.id = mapID;
            return mapData;
        }

        public LIMetadata GetMetadata(string mapID)
        {
            if (!Exists(mapID))
            {
                LILogger.Error("Could not find [" + mapID + "] in filesystem");
                return null;
            }
            LILogger.Info("Loading map [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            string mapJson = File.ReadAllText(mapPath);
            LIMetadata mapData = JsonSerializer.Deserialize<LIMetadata>(mapJson);
            mapData.id = mapID;
            return mapData;
        }

        public void GetAsync(string mapID, System.Action<LIMap> callback)
        {
            StartCoroutine(CoGetAsync(mapID, callback).WrapToIl2Cpp());
        }

        public IEnumerator CoGetAsync(string mapID, System.Action<LIMap> callback)
        {
            if (!Exists(mapID))
            {
                LILogger.Error("Could not find [" + mapID + "] in filesystem");
                yield break;
            }
            LILogger.Info("Loading map [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            UnityWebRequest request = UnityWebRequest.Get("file://" + mapPath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                LILogger.Error(request.error);
            }
            else
            {
                LIMap mapData = JsonSerializer.Deserialize<LIMap>(request.downloadHandler.text);
                mapData.id = mapID;
                callback(mapData);
            }
            request.Dispose();
        }

        public void Save(LIMap map)
        {
            LILogger.Info("Saving [" + map.id + "] to filesystem");
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            string mapPath = GetPath(map.id);
            string mapJson = JsonSerializer.Serialize(map, serializerOptions);
            if (!System.IO.Directory.Exists(GetDirectory()))
                System.IO.Directory.CreateDirectory(GetDirectory());
            System.IO.File.WriteAllText(mapPath, mapJson);
        }

        public void Delete(string mapID)
        {
            LILogger.Info("Deleting [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            System.IO.File.Delete(mapPath);
        }
    }
}