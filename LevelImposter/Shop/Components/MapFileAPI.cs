using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to manage LIM files in the local filesystem
    /// </summary>
    public class MapFileAPI : MonoBehaviour
    {
        public MapFileAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        public const float MIN_FRAMERATE = 20.0f;

        public static MapFileAPI? Instance = null;

        private Stopwatch? _loadTimer = new();
        private int _loadCount = 0;
        private bool _shouldLoad
        {
            get { return _loadTimer?.ElapsedMilliseconds <= (1000.0f / MIN_FRAMERATE); }
        }


        /// <summary>
        /// Gets the current directory where LevelImposter map files are stored.
        /// Usually in a LevelImposter folder beside the LevelImposter.dll.
        /// </summary>
        /// <returns>String path where LevelImposter data is stored.</returns>
        public string GetDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
            return Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter");
        }

        /// <summary>
        /// Gets the path where a specific map LIM file is stored.
        /// </summary>
        /// <param name="mapID">ID of the map file</param>
        /// <returns>The path where a specific map is stored</returns>
        public string GetPath(string mapID)
        {
            return Path.Combine(GetDirectory(), mapID + ".lim");
        }

        /// <summary>
        /// Lists all map file IDs that are located in the LevelImposter folder.
        /// </summary>
        /// <returns>Array of map file IDs that are located in the LevelImpsoter folder.</returns>
        [HideFromIl2Cpp]
        public string[] ListIDs()
        {
            string[] fileNames = Directory.GetFiles(GetDirectory(), "*.lim");
            for (int i = 0; i < fileNames.Length; i++)
                fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
            return fileNames;
        }

        /// <summary>
        /// Checks the existance of a map file based on ID
        /// </summary>
        /// <param name="mapID">Map File ID</param>
        /// <returns>True if a map file with the cooresponding ID exists</returns>
        public bool Exists(string? mapID)
        {
            if (mapID == null)
                return false;
            return File.Exists(GetPath(mapID));
        }

        /// <summary>
        /// Reads and parses a map file into memory.
        /// </summary>
        /// <param name="mapID">Map ID to read and parse</param>
        /// <param name="callback">Callback on success</param>
        /// <returns>Representation of the map file data in the form of a <c>LIMap</c>.</returns>
        [HideFromIl2Cpp]
        public void Get(string mapID, Action<LIMap?> callback)
        {
            StartCoroutine(CoGet(mapID, callback).WrapToIl2Cpp());
        }

        /// <summary>
        /// Reads and parses the metadata of a map file into memory.
        /// Less memory-intensive than <c>MapFileAPI.Get()</c>.
        /// </summary>
        /// <param name="mapID">Map ID to read and parse</param>
        /// <param name="callback">Callback on success</param>
        /// <returns>Representation of the map file data in the form of a <c>LIMetadata</c>.</returns>
        [HideFromIl2Cpp]
        public void GetMetadata(string mapID, Action<LIMetadata?> callback)
        {
            StartCoroutine(CoGet(mapID, callback).WrapToIl2Cpp());
        }

        /// <summary>
        /// Coroutine to grab map data from local filesystem
        /// </summary>
        /// <typeparam name="T">Output type, extends <c>LIMetadata</c></typeparam>
        /// <param name="mapID">Map ID to read and parse</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        private IEnumerator CoGet<T>(string mapID, Action<T?>? callback) where T : LIMetadata
        {
            {
                if (!Exists(mapID))
                {
                    LILogger.Error($"Could not find [{mapID}] in filesystem");
                    yield break;
                }
                LILogger.Info($"Loading map [{mapID}] from filesystem");
                _loadCount++;
                yield return null;

                // File Reader
                while (!_shouldLoad)
                    yield return null;
                string mapPath = GetPath(mapID);
                using (FileStream mapStream = File.OpenRead(mapPath))
                {
                    T? mapData = JsonSerializer.Deserialize<T>(mapStream);
                    if (mapData == null)
                    {
                        LILogger.Warn($"Invalid map data in [{mapID}]");
                    }
                    else
                    {
                        mapData.id = mapID;
                        if (callback != null)
                            callback(mapData);
                        mapData = null;
                    }
                    _loadCount--;
                }
                callback = null;
            }
        }

        /// <summary>
        /// Saves map data into the local filesystem based on the map's ID.
        /// </summary>
        /// <param name="map">Map data to save</param>
        [HideFromIl2Cpp]
        public void Save(LIMap map)
        {
            LILogger.Info($"Saving {map} to filesystem");
            JsonSerializerOptions serializerOptions = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string mapPath = GetPath(map.id);
            string mapJson = JsonSerializer.Serialize(map, serializerOptions);
            if (!Directory.Exists(GetDirectory()))
                Directory.CreateDirectory(GetDirectory());
            File.WriteAllText(mapPath, mapJson);
        }

        /// <summary>
        /// Deletes a map file from the filesystem.
        /// </summary>
        /// <param name="mapID">ID of the map to delete</param>
        public void Delete(string mapID)
        {
            LILogger.Info($"Deleting [{mapID}] from filesystem");
            string mapPath = GetPath(mapID);
            File.Delete(mapPath);
        }

        public void Awake()
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
        public void Start()
        {
            if (!Directory.Exists(GetDirectory()))
                Directory.CreateDirectory(GetDirectory());
        }
        public void Update()
        {
            if (_loadCount > 0)
                _loadTimer?.Restart();
        }
    }
}