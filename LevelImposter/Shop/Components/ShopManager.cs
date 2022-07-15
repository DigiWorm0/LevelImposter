
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

        private void Awake()
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

        private void Start()
        {
            MapAPI.GetMaps(OnMapsLoaded);
        }

        private void OnMapsLoaded(LIMetadata[] maps)
        {
            for (int i = 0; i < maps.Length; i++)
            {
                GameObject mapButton = Instantiate(mapButtonPrefab, mapButtonParent);
                mapButton.GetComponent<MapButton>().SetMap(maps[i]);
                //mapButton.transform.position += new Vector3(0, -i * 1.1f, 0);
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