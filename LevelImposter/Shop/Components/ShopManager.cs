using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LevelImposter.Core;
using LevelImposter.DB;
using InnerNet;

namespace LevelImposter.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        public MapBanner mapBannerPrefab;
        public Transform shopParent;

        private string currentList = "downloaded";

        public ShopManager(IntPtr intPtr) : base(intPtr)
        {
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ListDownloaded();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Close();
            }
        }

        public void Close()
        {
            if (SceneManager.GetActiveScene().name == "HowToPlay")
                SceneManager.LoadScene("MainMenu");
            else
                Destroy(gameObject);
        }

        public void ListNone()
        {
            currentList = "none";
            while (shopParent.childCount > 1)
                DestroyImmediate(shopParent.GetChild(1).gameObject);
        }

        public void ListDownloaded()
        {
            ListNone();
            currentList = "downloaded";
            string[] mapIDs = MapFileAPI.Instance.ListIDs();
            foreach (string mapID in mapIDs)
            {
                MapBanner banner = Instantiate(mapBannerPrefab, shopParent);
                banner.gameObject.SetActive(true);
                banner.SetMap(MapFileAPI.Instance.GetMetadata(mapID));
            }
        }

        public void ListTop()
        {
            ListNone();
            currentList = "top";
            LevelImposterAPI.Instance.GetTop((LIMetadata[] maps) =>
            {
                if (currentList != "top")
                    return;
                foreach (LIMetadata map in maps)
                {
                    MapBanner banner = Instantiate(mapBannerPrefab, shopParent);
                    banner.gameObject.SetActive(true);
                    banner.SetMap(map);
                }
            });
        }

        public void ListRecent()
        {
            ListNone();
            currentList = "recent";
            LevelImposterAPI.Instance.GetRecent((LIMetadata[] maps) =>
            {
                if (currentList != "recent")
                    return;
                foreach (LIMetadata map in maps)
                {
                    MapBanner banner = Instantiate(mapBannerPrefab, shopParent);
                    banner.gameObject.SetActive(true);
                    banner.SetMap(map);
                }
            });
        }

        public void ListFeatured()
        {
            ListNone();
            currentList = "featured";
            LevelImposterAPI.Instance.GetFeatured((LIMetadata[] maps) =>
            {
                if (currentList != "featured")
                    return;
                foreach (LIMetadata map in maps)
                {
                    MapBanner banner = Instantiate(mapBannerPrefab, shopParent);
                    banner.gameObject.SetActive(true);
                    banner.SetMap(map);
                }
            });
        }

        public void SelectMap(string id)
        {
            LILogger.Info("Selecting map [" + id + "]");
            MapLoader.LoadMap(id);
            CloseShop();
        }

        public void LaunchMap(string id)
        {
            if (!AssetDB.isReady)
            {
                DisconnectPopup.Instance.SendMessage("Hold up! The Asset DB isn't quite ready yet.");
                return;
            }
            LILogger.Info("Launching map [" + id + "]");
            MapLoader.LoadMap(id);

            AmongUsClient.Instance.TutorialMapId = 2;
            SaveManager.GameHostOptions.gameType = GameType.Normal;
            SoundManager.Instance.StopAllSound();
            AmongUsClient.Instance.GameMode = GameModes.FreePlay;
            DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
            AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023, false);
            AmongUsClient.Instance.MainMenuScene = "MainMenu";
            AmongUsClient.Instance.OnlineScene = "Tutorial";
            AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient, null);

            StartCoroutine(AmongUsClient.Instance.WaitForConnectionOrFail());
        }

        public static void CloseShop()
        {
            if (Instance != null)
                Instance.Close();
        }
    }
}