
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

        public void DownloadMap(LIMetadata map, Action callbackFinish)
        {
            MapAPI.DownloadMap(map.id, (System.Action<string>)((string mapJson) =>
            {
                string path = Path.Combine(Application.persistentDataPath, "maps", map.id + ".json");
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, mapJson);
                LILogger.Msg("Map downloaded to " + path);
                callbackFinish();
            }));
        }

        public void DeleteMap(Guid id)
        {
            string path = Path.Combine(Application.persistentDataPath, "maps", id + ".json");
            File.Delete(path);
            LILogger.Msg("Map deleted from " + path);
        }
    }
}