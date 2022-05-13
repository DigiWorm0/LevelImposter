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
        public const string API_PATH = "https://localhost:7108/api/";

        public static void GetMap(Guid mapId, Action<string> callback)
        {
            Request(API_PATH + "maps/" + mapId, callback);
        }

        public static void GetMaps(Action<LIMap[]> callback)
        {
            Request(API_PATH + "maps", (System.Action<string>)((string mapJson) =>
            {
                callback(System.Text.Json.JsonSerializer.Deserialize<LIMap[]>(mapJson));
            }));
        }

        private static void Request(string url, Action<string> callback)
        {
            LILogger.Msg("GET: " + url);
            var request = UnityWebRequest.Get(url);
            request.SendWebRequest().add_completed((System.Action<AsyncOperation>)((AsyncOperation op) =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    LILogger.Error(request.error);
                    return;
                }
                var json = request.downloadHandler.text;
                callback(json);
            }));
        }
    }
}