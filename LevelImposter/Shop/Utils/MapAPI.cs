using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using System.Text.Json;

namespace LevelImposter.Shop
{
    public static class MapAPI
    {
        public const string API_PATH = "https://us-central1-levelimposter-347807.cloudfunctions.net/api/";
        public const string GH_PATH = "https://api.github.com/repos/DigiWorm0/LevelImposter/releases?per_page=1";
        public const int API_VERSION = 1;

        public static void DownloadMap(Guid mapID, Action<string> callback)
        {
            GetMap(mapID, (LIMetadata metadata) =>
            {
                LILogger.Info("Downloading map [" + mapID + "]...");
                Request(metadata.downloadURL, (string mapJSON) =>
                {
                    LILogger.Info("Parsing map [" + mapID + "]...");
                    LIMap mapData = JsonSerializer.Deserialize<LIMap>(mapJSON);

                    mapData.v = metadata.v;
                    mapData.id = mapID.ToString();
                    mapData.name = metadata.name;
                    mapData.description = metadata.description;
                    mapData.authorID = metadata.authorID;
                    mapData.authorName = metadata.authorName;
                    mapData.isPublic = metadata.isPublic;
                    mapData.isVerified = metadata.isVerified;
                    mapData.createdAt = metadata.createdAt;

                    JsonSerializerOptions serializerOptions = new JsonSerializerOptions {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    };

                    string mapJson = JsonSerializer.Serialize(mapData, serializerOptions);
                    callback(mapJson);
                });
            });
        }

        public static void GetMap(Guid mapID, Action<LIMetadata> callback)
        {
            LILogger.Info("Getting map [" + mapID + "]...");
            RequestJson(API_PATH + "map/" + mapID, callback);
        }

        public static void GetRecentMaps(Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting map listing...");
            RequestJson(API_PATH + "maps/recent", callback);
        }

        public static void GetVerifiedMaps(Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting map listing...");
            RequestJson(API_PATH + "maps/verified", callback);
        }

        public static void GetTopMaps(Action<LIMetadata[]> callback)
        {
            LILogger.Info("Getting map listing...");
            RequestJson(API_PATH + "maps/top", callback);
        }

        public static void GetUpdate(Action<LIUpdate> callback)
        {
            LILogger.Info("Getting update info...");
            Request(GH_PATH, (string json) =>
            {
                GHRelease[] ghReleases = JsonSerializer.Deserialize<GHRelease[]>(json);
                GHRelease ghRelease = ghReleases[0];

                LIUpdate liUpdate = new LIUpdate();
                liUpdate.name = ghRelease.name;
                liUpdate.tag = ghRelease.tag_name.Split("-")[0].Replace("v", "");
                liUpdate.downloadURL = ghRelease.assets[0].browser_download_url;
                callback(liUpdate);
            });
        }

        public static void DownloadUpdate(Action<string> callback)
        {
            GetUpdate((LIUpdate liUpdate) =>
            {
                LILogger.Info("Downloading update [" + liUpdate.tag + "]...");
                Request(liUpdate.downloadURL, callback);
            });
        }

        private static void RequestJson<T>(string url, Action<T> callback)
        {
            Request(url, ((string json) =>
            {
                LICallback<T> response = JsonSerializer.Deserialize<LICallback<T>>(json);
                if (response.v != API_VERSION)
                    HandleError("You are running on an older version of LevelImposter (" + LevelImposter.VERSION + "). Update to get access to the API.");
                else if (!string.IsNullOrEmpty(response.error))
                    HandleError(response.error);
                else
                    callback(response.data);
            }));
        }

        private static void HandleError(string error)
        {
            LILogger.Error(error);
            DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(error);
        }

        private static void Request(string url, Action<string> callback)
        {
            LILogger.Info("GET: " + url);
            var request = UnityWebRequest.Get(url);
            request.SendWebRequest().add_completed((Action<AsyncOperation>)((AsyncOperation op) =>
            {
                LILogger.Info("RESPONSE: " + request.responseCode);
                if (request.isNetworkError || request.isHttpError)
                    HandleError(request.error);
                else
                {
                    var data = request.downloadHandler.text;
                    callback(data);
                }
            }));
        }
    }
}