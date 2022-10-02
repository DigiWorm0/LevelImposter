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
        public MapFileAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        public static MapFileAPI Instance;

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

        /// <summary>
        /// Gets the current directory where LevelImposter map files are stored.
        /// Usually in a LevelImposter folder beside the LevelImposter.dll.
        /// </summary>
        /// <returns>String path where LevelImposter data is stored.</returns>
        public string GetDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter)).Location;
            return Path.Combine(Path.GetDirectoryName(gameDir), "LevelImposter");
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
        public string[] ListIDs()
        {
            string[] fileNames = System.IO.Directory.GetFiles(GetDirectory(), "*.lim");
            for (int i = 0; i < fileNames.Length; i++)
                fileNames[i] = System.IO.Path.GetFileNameWithoutExtension(fileNames[i]);
            return fileNames;
        }

        /// <summary>
        /// Checks the existance of a map file based on ID
        /// </summary>
        /// <param name="mapID">Map File ID</param>
        /// <returns>True if a map file with the cooresponding ID exists</returns>
        public bool Exists(string mapID)
        {
            return System.IO.File.Exists(GetPath(mapID));
        }

        /// <summary>
        /// Reads and parses a map file into memory.
        /// </summary>
        /// <param name="mapID">Map ID to read and parse</param>
        /// <returns>Representation of the map file data in the form of a <c>LIMap</c>.</returns>
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

        /// <summary>
        /// Reads and parses the metadata of a map file into memory.
        /// Less memory-intensive than <c>MapFileAPI.Get()</c>.
        /// </summary>
        /// <param name="mapID">Map ID to read and parse</param>
        /// <returns>Representation of the map file data in the form of a <c>LIMetadata</c>.</returns>
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

        /// <summary>
        /// Saves map data into the local filesystem based on the map's ID.
        /// </summary>
        /// <param name="map">Map data to save</param>
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

        /// <summary>
        /// Deletes a map file from the filesystem.
        /// </summary>
        /// <param name="mapID">ID of the map to delete</param>
        public void Delete(string mapID)
        {
            LILogger.Info("Deleting [" + mapID + "] from filesystem");
            string mapPath = GetPath(mapID);
            System.IO.File.Delete(mapPath);
        }
    }
}