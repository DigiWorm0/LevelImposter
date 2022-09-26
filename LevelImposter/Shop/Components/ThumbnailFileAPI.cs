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
        public const int TEX_WIDTH = 412;
        public const int TEX_HEIGHT = 144;

        public static ThumbnailFileAPI Instance;

        public ThumbnailFileAPI(IntPtr intPtr) : base(intPtr)
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
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(gameDir), "LevelImposter/Thumbnails");
        }

        public string GetPath(string mapID)
        {
            return System.IO.Path.Combine(GetDirectory(), mapID + ".png");
        }

        public bool Exists(string mapID)
        {
            return System.IO.File.Exists(GetPath(mapID));
        }

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

        public void Save(string mapID, byte[] thumbnailData)
        {
            LILogger.Info("Saving [" + mapID + "] thumbnail to filesystem");
            string thumbnailPath = GetPath(mapID);
            if (!System.IO.Directory.Exists(GetDirectory()))
                System.IO.Directory.CreateDirectory(GetDirectory());
            System.IO.File.WriteAllBytes(thumbnailPath, thumbnailData);
        }

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