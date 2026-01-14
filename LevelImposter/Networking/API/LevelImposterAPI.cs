using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using Sentry.Protocol;
using UnityEngine;

namespace LevelImposter.Networking.API;

/// <summary>
///     API to send/recieve map files from LevelImposter.net
/// </summary>
public static class LevelImposterAPI
{
    public const string API_PATH = "https://api.levelimposter.net";
    public const int API_VERSION = 1;

    /// <summary>
    ///     Runs an HTTP request on a LevelImposter API
    ///     w/ a JSON-encoded <c>APIResponse</c>.
    /// </summary>
    /// <typeparam name="T">
    ///     <c>Data type the APIResponse uses</c>
    /// </typeparam>
    /// <param name="url">URL to request</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void Request<T>(string url, Action<T> onSuccess, Action<string>? onError)
    {
        HTTPHandler.RequestJSON<APIResponse<T>>(url, result =>
        {
            // Validate response
            if (result.ErrorText != null)
                onError?.Invoke(result.ErrorText);

            else if (result.Data == null)
                onError?.Invoke("Invalid API Response");

            else if (result.Data.Version != API_VERSION)
                onError?.Invoke($"You are running on an older version of LevelImposter {LevelImposter.DisplayVersion}. Update to get access to the API.");
                
            else if (!string.IsNullOrEmpty(result.Data.Error))
                onError?.Invoke(result.Data.Error);

            else if (result.Data.Data == null)
                onError?.Invoke("API returned no data");

            else
                onSuccess(result.Data.Data);
        });
    }

    /// <summary>
    ///     Grabs the top liked maps from the LevelImposter API.
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void GetTop(Action<LIMetadata[]> onSuccess, Action<string> onError)
    {
        LILogger.Info("Getting top maps...");
        Request(API_PATH + "/maps/top", onSuccess, onError);
    }

    /// <summary>
    ///     Grabs the most recent maps from the LevelImposter API.
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void GetRecent(Action<LIMetadata[]> onSuccess, Action<string> onError)
    {
        LILogger.Info("Getting recent maps...");
        Request(API_PATH + "/maps/recent", onSuccess, onError);
    }

    /// <summary>
    ///     Grabs the featured maps from the LevelImposter API.
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void GetFeatured(Action<LIMetadata[]> onSuccess, Action<string> onError)
    {
        LILogger.Info("Getting verified maps...");
        Request(API_PATH + "/maps/verified", onSuccess, onError);
    }

    /// <summary>
    ///     Grabs specific map metadata from the LevelImposter API.
    /// </summary>
    /// <param name="id">ID of the map to grab</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void GetMap(Guid id, Action<LIMetadata> onSuccess, Action<string>? onError)
    {
        LILogger.Info($"Getting map [{id}]...");
        Request(API_PATH + "/map/" + id, onSuccess, onError);
    }

    /// <summary>
    ///     Downloads specific map data from the LevelImposter API.
    /// </summary>
    /// <param name="id">ID of the map to download</param>
    /// <param name="downloadPath">Path to download the map to</param>
    /// <param name="onProgress">Callback on download progress</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public static void DownloadMap(
        Guid id,
        string downloadPath,
        Action<float>? onProgress,
        Action<FileStore> onSuccess,
        Action<string>? onError)
    {
        LILogger.Info($"Downloading map [{id}]...");
        
        GetMap(id, metadata => HTTPHandler.DownloadFile(
            metadata.downloadURL,
            downloadPath,
            onProgress,
            result =>
            {
                if (result.ErrorText != null)
                    onError?.Invoke(result.ErrorText);
                else if (result.Data == null)
                    onError?.Invoke("Unknown error downloading map");
                else
                    onSuccess(result.Data);
            }
        ), null);
    }

    /// <summary>
    ///     Downloads a specific map thumbnail from the LevelImposter API.
    /// </summary>
    /// <param name="metadata">Metadata of the map to grab a thumbnail for</param>
    /// <param name="onSuccess">Callback on success</param>
    [HideFromIl2Cpp]
    public static void DownloadThumbnail(
        string mapID,
        string downloadPath,
        Action<Sprite> onSuccess,
        Action<string>? onError = null)
    {
        // Prepare download URL
        var downloadURL = $"{API_PATH}/map/{mapID}/thumbnail";
        
        // Download Thumbnail from API
        HTTPHandler.DownloadFile(
            downloadURL,
            downloadPath,
            null,
            // On load, load the sprite from filesystem
            (result) => {
                if (result.ErrorText != null)
                    onError?.Invoke(result.ErrorText);
                else if (result.Data == null)
                    onError?.Invoke("Unknown error downloading thumbnail");
                else
                    SpriteLoader.LoadAsync($"{mapID}_thumb", result.Data, onSuccess);
            }
        );
    }

    /// <summary>
    ///    Generic callback wrapper for LevelImposter API responses
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    [Serializable]
    public class APIResponse<T>
    {
        /// <summary>
        ///   API version of the response. Should match <c>LevelImposterAPI.API_VERSION</c>
        /// </summary>
        [JsonPropertyName("v")] public int Version { get; set; }

        /// <summary>
        ///   Error message, if any.
        ///   If this is set, <c>data</c> will be null.
        ///   If this is null or empty, <c>data</c> will be set.
        /// </summary>
        [JsonPropertyName("error")] public string? Error { get; set; }

        /// <summary>
        ///    Data returned from the API.
        /// </summary>
        [JsonPropertyName("data")] public T? Data { get; set; }
    }
}