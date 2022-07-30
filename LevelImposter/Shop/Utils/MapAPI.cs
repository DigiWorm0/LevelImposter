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

        public static void DownloadMap(Guid mapID, Action<string> callback)
        {
            GetMap(mapID, ((LIMetadata metadata) =>
            {
                LILogger.Info("Downloading map [" + mapID + "]...");
                RequestJson(metadata.downloadURL[0], (LIMap mapData) =>
                {
                    LILogger.Info("Parsing map [" + mapID + "]...");
                    mapData.v = metadata.v;
                    mapData.id = mapID.ToString();
                    mapData.name = metadata.name;
                    mapData.description = metadata.description;
                    mapData.authorID = metadata.authorID;
                    mapData.authorName = metadata.authorName;
                    mapData.isPublic = metadata.isPublic;
                    mapData.isVerified = metadata.isVerified;

                    string mapJson = System.Text.Json.JsonSerializer.Serialize(mapData);
                    callback(mapJson);
                });
            }));
        }

        public static void GetMap(Guid mapID, Action<LIMetadata> callback)
        {
            LILogger.Info("Getting map [" + mapID + "]...");
            RequestJson(API_PATH + "map/" + mapID, callback);
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