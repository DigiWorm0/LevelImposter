using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using Newtonsoft.Json;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to manage thumbnail files in the local filesystem
    /// </summary>
    public class ThumbnailFileAPI : MonoBehaviour
    {
        public ThumbnailFileAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        public static ThumbnailFileAPI? Instance = null;

        /// <summary>
        /// Gets the current directory where LevelImposter thumbnail files are stored.
        /// Usually in a sub-directory within the LevelImposter folder beside the LevelImposter.dll.
        /// </summary>
        /// <returns>String path where LevelImposter map thumbnails is stored.</returns>
        public string GetDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
            return Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter/Thumbnails");
        }

        /// <summary>
        /// Gets the path where a specific map thumbnail file is stored.
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <returns>The path where a specific map thumbnail file is stored</returns>
        public string GetPath(string mapID)
        {
            return Path.Combine(GetDirectory(), mapID + ".png");
        }

        /// <summary>
        /// Checks the existance of a map thumbnail file based on ID
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <returns>True if a map thumbnail file with the cooresponding ID exists</returns>
        public bool Exists(string mapID)
        {
            return File.Exists(GetPath(mapID));
        }

        /// <summary>
        /// Reads and parses a thumbnail file into a Texture2D.
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void Get(string mapID, Action<Sprite?> callback)
        {
            if (!Exists(mapID))
            {
                LILogger.Warn($"Could not find [{mapID}] thumbnail in filesystem");
                return;
            }

            LILogger.Info($"Loading thumbnail [{mapID}] from filesystem");
            string thumbnailPath = GetPath(mapID);
            byte[] thumbnailBytes = File.ReadAllBytes(thumbnailPath);
            SpriteLoader.Instance?.LoadSpriteAsync(thumbnailBytes, false, (spriteData) =>
            {
                Sprite? sprite = spriteData?.Sprite;
                if (sprite == null)
                {
                    LILogger.Warn($"Error loading [{mapID}] thumbnail from filesystem");
                    return;
                }
                callback.Invoke(spriteData?.Sprite);
            });
        }

        /// <summary>
        /// Saves a map thumbnail file to the local filesystem.
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <param name="thumbnailData">PNG-encoded image data.</param>
        [HideFromIl2Cpp]
        public void Save(string mapID, byte[] thumbnailBytes)
        {
            LILogger.Info($"Saving [{mapID}] thumbnail to filesystem");
            string thumbnailPath = GetPath(mapID);
            if (!Directory.Exists(GetDirectory()))
                Directory.CreateDirectory(GetDirectory());
            File.WriteAllBytes(thumbnailPath, thumbnailBytes);
        }

        /// <summary>
        /// Deletes a map thumbnail file from the local filesystem
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        public void Delete(string mapID)
        {
            if (!Exists(mapID))
                return;
            LILogger.Info($"Deleting [{mapID}] thumbnail from filesystem");
            string mapPath = GetPath(mapID);
            File.Delete(mapPath);
        }

        /// <summary>
        /// Deletes all thumbnail files from the local filesystem
        /// </summary>
        public void DeleteAll()
        {
            DirectoryInfo directory = new DirectoryInfo(GetDirectory());
            if (!directory.Exists)
                return;

            LILogger.Info("Deleting all thumbnails from filesystem");
            directory.Delete(true);
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
    }
}