using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using Newtonsoft.Json;
using BepInEx.IL2CPP.Utils.Collections;

namespace LevelImposter.Shop
{
    public class ThumbnailFileAPI : MonoBehaviour
    {
        public ThumbnailFileAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        public const int TEX_WIDTH = 412;
        public const int TEX_HEIGHT = 144;

        public static ThumbnailFileAPI Instance;

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
        /// Gets the current directory where LevelImposter thumbnail files are stored.
        /// Usually in a sub-directory within the LevelImposter folder beside the LevelImposter.dll.
        /// </summary>
        /// <returns>String path where LevelImposter map thumbnails is stored.</returns>
        public string GetDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter)).Location;
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(gameDir), "LevelImposter/Thumbnails");
        }

        /// <summary>
        /// Gets the path where a specific map thumbnail file is stored.
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <returns>The path where a specific map thumbnail file is stored</returns>
        public string GetPath(string mapID)
        {
            return System.IO.Path.Combine(GetDirectory(), mapID + ".png");
        }

        /// <summary>
        /// Checks the existance of a map thumbnail file based on ID
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <returns>True if a map thumbnail file with the cooresponding ID exists</returns>
        public bool Exists(string mapID)
        {
            return System.IO.File.Exists(GetPath(mapID));
        }

        /// <summary>
        /// Reads and parses a thumbnail file into a Texture2D.
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <param name="callback">Callback on success</param>
        public void Get(string mapID, System.Action<Texture2D> callback)
        {
            if (!Exists(mapID))
            {
                LILogger.Error("Could not find [" + mapID + "] thumbnail in filesystem");
                return;
            }

            LILogger.Info("Loading thumbnail [" + mapID + "] from filesystem");
            string thumbnailPath = GetPath(mapID);
            byte[] thumbnailBytes = System.IO.File.ReadAllBytes(thumbnailPath);
            Texture2D texture = new Texture2D(1, 1);
            ImageConversion.LoadImage(texture, thumbnailBytes);
            callback(texture);
        }

        /// <summary>
        /// Saves a map thumbnail file to the local filesystem.
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        /// <param name="thumbnailData">PNG-encoded image data.</param>
        public void Save(string mapID, byte[] thumbnailData)
        {
            LILogger.Info("Saving [" + mapID + "] thumbnail to filesystem");
            string thumbnailPath = GetPath(mapID);
            if (!System.IO.Directory.Exists(GetDirectory()))
                System.IO.Directory.CreateDirectory(GetDirectory());
            System.IO.File.WriteAllBytes(thumbnailPath, thumbnailData);
        }

        /// <summary>
        /// Deletes a map thumbnail file from the local filesystem
        /// </summary>
        /// <param name="mapID">Map ID for the thumbnail</param>
        public void Delete(string mapID)
        {
            if (!Exists(mapID))
                return;
            LILogger.Info("Deleting [" + mapID + "] thumbnail from filesystem");
            string mapPath = GetPath(mapID);
            System.IO.File.Delete(mapPath);
        }
    }
}