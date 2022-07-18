
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
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Start()
        {
            MapAPI.GetMaps(OnMapsLoaded);
            scroller = GetComponent<Scroller>();
        }

        private void OnMapsLoaded(LIMetadata[] maps)
        {
            scroller.ContentYBounds = new FloatRange(-1.8f, (1.1f * maps.Length) - 1.8f);
            for (int i = 0; i < maps.Length; i++)
            {
                GameObject mapButton = Instantiate(mapButtonPrefab, mapButtonParent);
                mapButton.transform.localPosition = new Vector3(0, i * -1.1f + 1.8f, 0);
                mapButton.GetComponent<MapButton>().SetMap(maps[i]);
            }
        }

        public void DownloadMap(Guid mapID, Action callbackFinish)
        {
            MapAPI.DownloadMap(mapID, (System.Action<string>)((string mapJson) =>
            {
                MapLoader.WriteMap(mapID, mapJson);
                callbackFinish();
            }));
        }

        public void DeleteMap(Guid id)
        {
            MapLoader.DeleteMap(id);
        }
    }
}