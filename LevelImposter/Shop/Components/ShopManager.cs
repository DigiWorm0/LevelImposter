
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public GameObject mapButtonPrefab;
        public Transform mapButtonParent;

        public static ShopManager Instance { get; private set; }

        private Scroller scroller;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            scroller = GetComponent<Scroller>();
            ListPublicMaps();
        }

        public void ListDownloadedMaps()
        {
            LILogger.Info("Using downloaded maps...");
            string[] mapIDs = MapLoader.GetMapIDs();
            LIMetadata[] maps = new LIMetadata[mapIDs.Length];
            for (int i = 0; i < mapIDs.Length; i++)
            {
                maps[i] = MapLoader.GetMap(mapIDs[i]);
                maps[i].id = mapIDs[i];
            }
            OnMapsLoaded(maps);
        }

        public void ListPublicMaps()
        {
            LILogger.Info("Using public maps...");
            MapAPI.GetMaps(OnMapsLoaded);
        }

        private void OnMapsLoaded(LIMetadata[] maps)
        {
            LILogger.Info("Listed " + maps.Length + " maps");
            while (mapButtonParent.childCount > 1)
                DestroyImmediate(mapButtonParent.GetChild(1).gameObject);
            scroller.ContentYBounds = new FloatRange(-1.8f, (1.1f * maps.Length) - 1.8f);
            for (int i = 0; i < maps.Length; i++)
            {
                GameObject mapButton = Instantiate(mapButtonPrefab, mapButtonParent);
                mapButton.transform.localPosition = new Vector3(0, i * -1.1f + 1.8f, -1);
                mapButton.GetComponent<MapButton>().SetMap(maps[i]);
            }
        }

        public void DownloadMap(Guid mapID, Action callbackFinish)
        {
            MapAPI.DownloadMap(mapID, (System.Action<string>)((string mapJson) =>
            {
                MapLoader.WriteMap(mapID.ToString(), mapJson);
                callbackFinish();
            }));
        }

        public void DeleteMap(string id)
        {
            MapLoader.DeleteMap(id.ToString());
        }
    }
}