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
        public ShopManager(IntPtr intPtr) : base(intPtr)
        {
        }

        public static ShopManager Instance { get; private set; }
        public static bool IsOpen = false; // Circumvent Garbage Collection Issues w/ ShopManager.Instance

        public MapBanner MapBannerPrefab;
        public Transform ShopParent;

        private string _currentListID = "downloaded";
        private HostLocalGameButton _freeplayComp;

        public void Awake()
        {
            Instance = this;
            IsOpen = true;
            ControllerManager.Instance.OpenOverlayMenu("LIShop", null);
        }

        public void Start()
        {
            _freeplayComp = gameObject.AddComponent<HostLocalGameButton>();
            _freeplayComp.GameMode = GameModes.FreePlay;
            ListDownloaded();
        }

        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Close();
            }
        }

        public void OnDestroy()
        {
            IsOpen = false;
            ControllerManager.Instance.CloseOverlayMenu("LIShop");
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
            _currentListID = "none";
            while (ShopParent.childCount > 1)
                DestroyImmediate(ShopParent.GetChild(1).gameObject);
        }

        public void ListDownloaded()
        {
            ListNone();
            _currentListID = "downloaded";
            string[] mapIDs = MapFileAPI.Instance.ListIDs();
            foreach (string mapID in mapIDs)
            {
                MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                banner.gameObject.SetActive(true);
                banner.SetMap(MapFileAPI.Instance.GetMetadata(mapID));
            }
        }

        public void ListTop()
        {
            ListNone();
            _currentListID = "top";
            LevelImposterAPI.Instance.GetTop((LIMetadata[] maps) =>
            {
                if (_currentListID != "top")
                    return;
                foreach (LIMetadata map in maps)
                {
                    MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                    banner.gameObject.SetActive(true);
                    banner.SetMap(map);
                }
            });
        }

        public void ListRecent()
        {
            ListNone();
            _currentListID = "recent";
            LevelImposterAPI.Instance.GetRecent((LIMetadata[] maps) =>
            {
                if (_currentListID != "recent")
                    return;
                foreach (LIMetadata map in maps)
                {
                    MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                    banner.gameObject.SetActive(true);
                    banner.SetMap(map);
                }
            });
        }

        public void ListFeatured()
        {
            ListNone();
            _currentListID = "featured";
            LevelImposterAPI.Instance.GetFeatured((LIMetadata[] maps) =>
            {
                if (_currentListID != "featured")
                    return;
                foreach (LIMetadata map in maps)
                {
                    MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                    banner.gameObject.SetActive(true);
                    banner.SetMap(map);
                }
            });
        }

        public void SelectMap(string id)
        {
            LILogger.Info("Selecting map [" + id + "]");
            MapLoader.LoadMap(id);
            MapUtils.SyncMapID();
            CloseShop();
        }

        public void LaunchMap(string id)
        {
            if (!AssetDB.IsReady)
                return;
            LILogger.Info("Launching map [" + id + "]");
            MapLoader.LoadMap(id);

            AmongUsClient.Instance.TutorialMapId = 2;
            _freeplayComp.OnClick();

            /*
            SaveManager.GameHostOptions.gameType = GameType.Normal;
            SoundManager.Instance.StopAllSound();
            AmongUsClient.Instance.GameMode = GameModes.FreePlay;
            DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
            AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023, false);
            AmongUsClient.Instance.MainMenuScene = "MainMenu";
            AmongUsClient.Instance.OnlineScene = "Tutorial";
            AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient, null);
            StartCoroutine(AmongUsClient.Instance.WaitForConnectionOrFail());
            */
        }

        public static void CloseShop()
        {
            if (Instance != null)
                Instance.Close();
        }
    }
}