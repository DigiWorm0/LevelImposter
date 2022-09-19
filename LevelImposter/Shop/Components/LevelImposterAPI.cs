using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using BepInEx.IL2CPP.Utils.Collections;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public class LevelImposterAPI : MonoBehaviour
    {
        public const string API_PATH = "https://us-central1-levelimposter-347807.cloudfunctions.net/api/";
        private const int API_VERSION = 1;

        public static LevelImposterAPI Instance;

        public LevelImposterAPI(IntPtr intPtr) : base(intPtr)
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

        public void Request(string url, System.Action<string> callback)
        {
            LILogger.Info("GET: " + url);
            StartCoroutine(CoRequest(url, callback).WrapToIl2Cpp());
        }

        public IEnumerator CoRequest(string url, System.Action<string> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                LILogger.Error(request.error);
            else
                callback(request.downloadHandler.text);
            request.Dispose();
        }

        public void RequestTexture(string url, System.Action<Texture2D> callback)
        {
            LILogger.Info("GET: " + url);
            StartCoroutine(CoRequestTexture(url, callback).WrapToIl2Cpp());
        }

        public IEnumerator CoRequestTexture(string url, System.Action<Texture2D> callback)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                LILogger.Error(request.error);
            else
                callback(DownloadHandlerTexture.GetContent(request));
            request.Dispose();
        }

        public void RequestJSON<T>(string url, System.Action<T> callback)
        {
            Request(url, (string json) =>
            {
                LICallback<T> response = JsonSerializer.Deserialize<LICallback<T>>(json);
                if (response.v != API_VERSION)
                    LILogger.Error("You are running on an older version of LevelImposter " + LevelImposter.VERSION + ". Update to get access to the API.");
                else if (!string.IsNullOrEmpty(response.error))
                    LILogger.Error(response.error);
                else
                    callback(response.data);
            });
        }

        public void GetTop(System.Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting top maps...");
            RequestJSON<LIMetadata[]>(API_PATH + "maps/top", callback);
        }

        public void GetRecent(System.Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting recent maps...");
            RequestJSON<LIMetadata[]>(API_PATH + "maps/recent", callback);
        }

        public void GetFeatured(System.Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting verified maps...");
            RequestJSON<LIMetadata[]>(API_PATH + "maps/verified", callback);
        }

        public void GetMap(System.Guid id, System.Action<LIMetadata> callback)
        {
            LILogger.Info("Getting map " + id + "...");
            RequestJSON<LIMetadata>(API_PATH + "map/" + id, callback);
        }

        public void DownloadMap(System.Guid id, System.Action<LIMap> callback)
        {
            LILogger.Info("Downloading map " + id + "...");
            GetMap(id, (LIMetadata metadata) =>
            {
                Request(metadata.downloadURL, (string mapJson) =>
                {
                    LILogger.Info("Parsing map " + id + "...");
                    LIMap mapData = JsonSerializer.Deserialize<LIMap>(mapJson);
                    mapData.v = metadata.v;
                    mapData.id = metadata.id;
                    mapData.name = metadata.name;
                    mapData.description = metadata.description;
                    mapData.authorID = metadata.authorID;
                    mapData.authorName = metadata.authorName;
                    mapData.isPublic = metadata.isPublic;
                    mapData.isVerified = metadata.isVerified;
                    mapData.createdAt = metadata.createdAt;
                    mapData.thumbnailURL = metadata.thumbnailURL;

                    callback(mapData);
                });
            });
        }

        public void DownloadThumbnail(LIMetadata metadata, System.Action<Texture2D> callback)
        {
            LILogger.Info("Downloading thumbnail for map " + metadata.id + "...");
            RequestTexture(metadata.thumbnailURL, (Texture2D thumbnail) =>
            {
                if (thumbnail.width != ThumbnailFileAPI.TEX_WIDTH || thumbnail.height != ThumbnailFileAPI.TEX_HEIGHT)
                {
                    LILogger.Error("Thumbnail for map " + metadata.id + " is not the correct size.");
                    return;
                }
                callback(thumbnail);
            });
        }
    }
}