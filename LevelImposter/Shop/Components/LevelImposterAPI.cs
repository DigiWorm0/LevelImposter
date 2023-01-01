using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Text.Json;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to send/recieve map files from LevelImposter.net
    /// </summary>
    public class LevelImposterAPI : MonoBehaviour
    {
        public LevelImposterAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        public const string API_PATH = "https://us-central1-levelimposter-347807.cloudfunctions.net/api/";
        public const int API_VERSION = 1;

        public static LevelImposterAPI? Instance = null;

        /// <summary>
        /// Runs an Async HTTP Request on a specific url.
        /// Handles and logs any errors.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void Request(string url, Action<string> callback)
        {
            StartCoroutine(CoRequest(url, callback).WrapToIl2Cpp());
        }

        /// <summary>
        /// Runs an Async HTTP Request on a
        /// specific url with a Unity Coroutine.
        /// Handles and logs any errors.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public IEnumerator CoRequest(string url, Action<string> callback)
        {
            LILogger.Info("GET: " + url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            LILogger.Info("RES: " + request.responseCode);

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                LILogger.Error(request.error);
            else
                callback(request.downloadHandler.text);
            request.Dispose();
        }

        /// <summary>
        /// Runs an HTTP Request for Texture2D data on a specific url.
        /// Handles and logs any errors.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void RequestTexture(string url, Action<Texture2D> callback)
        {
            StartCoroutine(CoRequestTexture(url, callback).WrapToIl2Cpp());
        }

        /// <summary>
        /// Runs an HTTP Request for Texture2D data
        /// on a specific url with a Unity Coroutine.
        /// Handles and logs any errors.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public IEnumerator CoRequestTexture(string url, Action<Texture2D> callback)
        {
            LILogger.Info("GET: " + url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            LILogger.Info("RES: " + request.responseCode);

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                LILogger.Error(request.error);
            else
            {
                Texture2D texture = new(1, 1);
                ImageConversion.LoadImage(texture, request.downloadHandler.data);
                callback(texture);
            }
            request.Dispose();
        }

        /// <summary>
        /// Runs an HTTP request on a LevelImposter API
        /// w/ a JSON-encoded <c>LICallback</c>.
        /// </summary>
        /// <typeparam name="T"><c>Data type the LICallback uses</c></typeparam>
        /// <param name="url">URL to request</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void RequestJSON<T>(string url, Action<T> callback)
        {
            Request(url, (string json) =>
            {
                LICallback<T>? response = JsonSerializer.Deserialize<LICallback<T>>(json);
                if (response == null)
                    LILogger.Error("Invalid API Response");
                else if (response.v != API_VERSION)
                    LILogger.Error("You are running on an older version of LevelImposter " + LevelImposter.Version + ". Update to get access to the API.");
                else if (!string.IsNullOrEmpty(response.error))
                    LILogger.Error(response.error);
                else
                    callback(response.data);
            });
        }

        /// <summary>
        /// Grabs the top liked maps from the LevelImposter API.
        /// </summary>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void GetTop(Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting top maps...");
            RequestJSON(API_PATH + "maps/top", callback);
        }

        /// <summary>
        /// Grabs the most recent maps from the LevelImposter API.
        /// </summary>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void GetRecent(Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting recent maps...");
            RequestJSON(API_PATH + "maps/recent", callback);
        }

        /// <summary>
        /// Grabs the featured maps from the LevelImposter API.
        /// </summary>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void GetFeatured(Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting verified maps...");
            RequestJSON(API_PATH + "maps/verified", callback);
        }

        /// <summary>
        /// Grabs specific map metadata from the LevelImposter API.
        /// </summary>
        /// <param name="id">ID of the map to grab</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void GetMap(Guid id, Action<LIMetadata> callback)
        {
            LILogger.Info("Getting map " + id + "...");
            RequestJSON(API_PATH + "map/" + id, callback);
        }

        /// <summary>
        /// Downloads specific map data from the LevelImposter API.
        /// </summary>
        /// <param name="id">ID of the map to download</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void DownloadMap(Guid id, Action<LIMap> callback)
        {
            LILogger.Info("Downloading map " + id + "...");
            GetMap(id, (LIMetadata metadata) =>
            {
                Request(metadata.downloadURL, (string mapJson) =>
                {
                    LILogger.Info("Parsing map " + id + "...");
                    LIMap? mapData = JsonSerializer.Deserialize<LIMap>(mapJson);
                    if (mapData == null)
                        return; // TODO: onError callback
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

        /// <summary>
        /// Downloads a specific map thumbnail from the LevelImposter API.
        /// </summary>
        /// <param name="metadata">Metadata of the map to grab a thumbnail for</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public void DownloadThumbnail(LIMetadata metadata, Action<Texture2D> callback)
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
    }
}