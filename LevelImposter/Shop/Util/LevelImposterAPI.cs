using System;
using System.IO;
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
    public static class LevelImposterAPI
    {
        public const string API_PATH = "https://us-central1-levelimposter-347807.cloudfunctions.net/api/";
        public const int API_VERSION = 1;

        /// <summary>
        /// Runs an HTTP request on a LevelImposter API
        /// w/ a JSON-encoded <c>LICallback</c>.
        /// </summary>
        /// <typeparam name="T"><c>Data type the LICallback uses</c></typeparam>
        /// <param name="url">URL to request</param>
        /// <param name="callback">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public static void Request<T>(string url, Action<T> callback, Action<string> onError)
        {
            HTTPHandler.Instance?.Request(url, (string json) =>
            {
                LICallback<T>? response = JsonSerializer.Deserialize<LICallback<T>>(json);
                if (response == null)
                    onError("Invalid API Response");
                else if (response.v != API_VERSION)
                    onError($"You are running on an older version of LevelImposter {LevelImposter.Version}. Update to get access to the API.");
                else if (!string.IsNullOrEmpty(response.error))
                    onError(response.error);
                else
                    callback(response.data);

                response = null;
                json = null;
                callback = null;
                url = null;
                onError = null;
            }, onError);
        }

        /// <summary>
        /// Grabs the top liked maps from the LevelImposter API.
        /// </summary>
        /// <param name="callback">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public static void GetTop(Action<LIMetadata[]> callback, Action<string> onError)
        {
            LILogger.Info("Getting top maps...");
            Request(API_PATH + "maps/top", callback, onError);
        }

        /// <summary>
        /// Grabs the most recent maps from the LevelImposter API.
        /// </summary>
        /// <param name="callback">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public static void GetRecent(Action<LIMetadata[]> callback, Action<string> onError)
        {
            LILogger.Info("Getting recent maps...");
            Request(API_PATH + "maps/recent", callback, onError);
        }

        /// <summary>
        /// Grabs the featured maps from the LevelImposter API.
        /// </summary>
        /// <param name="callback">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public static void GetFeatured(Action<LIMetadata[]> callback, Action<string> onError)
        {
            LILogger.Info("Getting verified maps...");
            Request(API_PATH + "maps/verified", callback, onError);
        }

        /// <summary>
        /// Grabs specific map metadata from the LevelImposter API.
        /// </summary>
        /// <param name="id">ID of the map to grab</param>
        /// <param name="callback">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public static void GetMap(Guid id, Action<LIMetadata> callback, Action<string> onError)
        {
            LILogger.Info($"Getting map {id}...");
            Request(API_PATH + "map/" + id, callback, onError);
        }

        /// <summary>
        /// Downloads specific map data from the LevelImposter API.
        /// </summary>
        /// <param name="id">ID of the map to download</param>
        /// <param name="onProgress">Callback on download progress</param>
        /// <param name="callback">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public static void DownloadMap(Guid id, Action<float>? onProgress, Action<LIMap> callback, Action<string> onError)
        {
            LILogger.Info($"Downloading map [{id}]...");
            GetMap(id, (LIMetadata metadata) =>
            {
                HTTPHandler.Instance?.Download(metadata.downloadURL, onProgress, (string mapJson) =>
                {
                    LILogger.Info($"Parsing map {metadata}...");
                    try
                    {
                        LIMap? mapData = JsonSerializer.Deserialize<LIMap>(mapJson);
                        mapJson = ""; // Free Memory
                        if (mapData == null)
                        {
                            onError("Map was null");
                            return;
                        }
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
                        mapData.remixOf = metadata.remixOf;

                        callback(mapData);
                        mapData = null;
                    }
                    catch (Exception e)
                    {
                        onError(e.ToString());
                    }
                }, onError);
            }, onError);
        }

        /// <summary>
        /// Downloads a specific map thumbnail from the LevelImposter API.
        /// </summary>
        /// <param name="metadata">Metadata of the map to grab a thumbnail for</param>
        /// <param name="callback">Callback on success</param>
        [HideFromIl2Cpp]
        public static void DownloadThumbnail(LIMetadata metadata, Action<Sprite> callback)
        {
            LILogger.Info($"Downloading thumbnail for map {metadata}...");
            HTTPHandler.Instance?.Request(metadata.thumbnailURL, (byte[] imgData) =>
            {
                ThumbnailCacheAPI.Instance?.Save(metadata.id, imgData);
                SpriteLoader.Instance?.LoadSpriteAsync(imgData, false, (spriteData) =>
                {
                    if (spriteData == null)
                    {
                        LILogger.Warn($"Error loading {metadata} thumbnail from API");
                        return;
                    }
                    Sprite? sprite = spriteData.Sprite;
                    if (sprite != null)
                        callback(sprite);
                }, metadata.id);
            }, null);
        }
    }
}