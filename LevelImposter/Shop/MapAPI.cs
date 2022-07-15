using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using Google.Cloud.Firestore;

namespace LevelImposter.Shop
{
    
    public static class MapAPI
    {
        public static FirestoreDb db { get; private set; }

        public static void Init()
        {
            db = FirestoreDb.Create("levelimposter-347807");
        }

        public static async void GetMap(Guid mapId, Action<LIMetadata> callback)
        {
            var docRef = db.Collection("maps").Document(mapId.ToString());
            var snapshot = await docRef.GetSnapshotAsync();
            var metadata = snapshot.ConvertTo<LIMetadata>();
            callback(metadata);
        }

        public static async void GetMaps(Action<LIMetadata[]> callback)
        {
            var collectionRef = db.Collection("maps");
            var snapshot = await collectionRef.GetSnapshotAsync();
            LIMetadata[] metadata = new LIMetadata[snapshot.Count];
            for (int i = 0; i < snapshot.Count; i++)
            {
                metadata[i] = snapshot[i].ConvertTo<LIMetadata>();
            }
            callback(metadata);
        }

        public static void GetMapData(LIMetadata metadata, Action<string> callback)
        {
            var request = UnityWebRequest.Get(metadata.storageURL);
            request.SendWebRequest().add_completed((System.Action<AsyncOperation>)((AsyncOperation op) =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    LILogger.Error(request.error);
                    return;
                }
                var json = request.downloadHandler.text;
                LILogger.Info(json);
                callback(json);
            }));
        }

    }
}