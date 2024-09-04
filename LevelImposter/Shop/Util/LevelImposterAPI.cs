using System;
using System.IO;
using System.Text.Json;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
///     API to send/recieve map files from LevelImposter.net
/// </summary>
public static class LevelImposterAPI
{
    public const string API_PATH = "https://api.levelimposter.net/";
    public const int API_VERSION = 1;

    /// <summary>
    ///     Runs an HTTP request on a LevelImposter API
    ///     w/ a JSON-encoded <c>LICallback</c>.
    /// </summary>
    /// <typeparam name="T">
    ///     <c>Data type the LICallback uses</c>
    /// </typeparam>
    /// <param name="url">URL to request</param>
    /// <param name="callback">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    public static void Request<T>(string url, Action<T> callback, Action<string> onError)
    {
        HTTPHandler.Instance?.Request(url, json =>
        {
            var response = JsonSerializer.Deserialize<LICallback<T>>(json);

            if (response == null)
                onError("Invalid API Response");
            else if (response.Version != API_VERSION)
                onError(
                    $"You are running on an older version of LevelImposter {LevelImposter.DisplayVersion}. Update to get access to the API.");
            else if (!string.IsNullOrEmpty(response.Error))
                onError(response.Error);
            else if (response.Data != null)
                callback(response.Data);
        }, onError);
    }

    /// <summary>
    ///     Grabs the top liked maps from the LevelImposter API.
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
    ///     Grabs the most recent maps from the LevelImposter API.
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
    ///     Grabs the featured maps from the LevelImposter API.
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
    ///     Grabs specific map metadata from the LevelImposter API.
    /// </summary>
    /// <param name="id">ID of the map to grab</param>
    /// <param name="callback">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    public static void GetMap(Guid id, Action<LIMetadata> callback, Action<string> onError)
    {
        LILogger.Info($"Getting map [{id}]...");
        Request(API_PATH + "map/" + id, callback, onError);
    }

    /// <summary>
    ///     Downloads specific map data from the LevelImposter API.
    /// </summary>
    /// <param name="id">ID of the map to download</param>
    /// <param name="onProgress">Callback on download progress</param>
    /// <param name="callback">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    public static void DownloadMap(Guid id, Action<float>? onProgress, Action<LIMap> callback, Action<string> onError)
    {
        LILogger.Info($"Downloading map [{id}]...");

        GetMap(id, metadata =>
        {
            HTTPHandler.Instance?.Download(metadata.downloadURL, onProgress, fileData =>
            {
                LILogger.Info($"Parsing map {id}...");
                try
                {
                    using (var memoryStream = new MemoryStream(fileData))
                    {
                        // Deserialize the map
                        var mapData = LIDeserializer.DeserializeMap(memoryStream);

                        // Free memory
                        fileData = null!;

                        // Check Download
                        if (mapData == null)
                        {
                            onError("Map was null");
                            return;
                        }

                        // Make Sure the ID Matches
                        mapData.id = id.ToString();

                        // Callback
                        callback(mapData);
                        mapData = null;
                    }
                }
                catch (Exception e)
                {
                    onError(e.Message);
                }
            }, onError);
        }, onError);
    }

    /// <summary>
    ///     Downloads a specific map thumbnail from the LevelImposter API.
    /// </summary>
    /// <param name="metadata">Metadata of the map to grab a thumbnail for</param>
    /// <param name="callback">Callback on success</param>
    [HideFromIl2Cpp]
    public static void DownloadThumbnail(LIMetadata metadata, Action<Sprite> callback)
    {
        LILogger.Info($"Downloading thumbnail for map {metadata}...");
        HTTPHandler.Instance?.Request(API_PATH + $"map/{metadata.id}/thumbnail", imgData =>
        {
            ThumbnailCache.Save(metadata.id, imgData);
            var imgStream = new MemoryStream(imgData);
            SpriteLoader.Instance?.LoadSpriteAsync(imgStream, spriteData =>
            {
                if (spriteData == null)
                {
                    LILogger.Warn($"Error loading {metadata} thumbnail from API");
                    return;
                }

                var sprite = spriteData.Sprite;
                if (sprite != null)
                    callback(sprite);
            }, metadata.id, null);
        }, null);
    }
}