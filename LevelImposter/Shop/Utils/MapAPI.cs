using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class MapAPI
    {
        public const string API_PATH = "https://us-central1-levelimposter-347807.cloudfunctions.net/api/";

        public static void DownloadMap(Guid mapId, Action<string> callback)
        {
            GetMap(mapId, (System.Action<LIMetadata>)((LIMetadata metadata) =>
            {
                LILogger.Info("Downloading map [" + mapId + "]...");
                Request(metadata.downloadURL[0], callback);
            }));
        }

        public static void GetMap(Guid mapId, Action<LIMetadata> callback)
        {
            LILogger.Info("Getting map [" + mapId + "]...");
            RequestJson(API_PATH + "map/" + mapId, callback);
        }

        public static void GetMaps(Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting map listing...");
            RequestJson(API_PATH + "maps", callback);
        }

        private static void RequestJson<T>(string url, Action<T> callback)
        {
            Request(url, (System.Action<string>)((string json) =>
            {
                T data = System.Text.Json.JsonSerializer.Deserialize<T>(json);
                callback(data);
            }));
        }

        private static void Request(string url, Action<string> callback)
        {
            LILogger.Info("GET: " + url);
            var request = UnityWebRequest.Get(url);
            request.SendWebRequest().add_completed((System.Action<AsyncOperation>)((AsyncOperation op) =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    LILogger.Error(request.error);
                    return;
                }
                LILogger.Info("RESPONSE: " + request.responseCode);
                var data = request.downloadHandler.text;
                callback(data);
            }));
        }
    }
}