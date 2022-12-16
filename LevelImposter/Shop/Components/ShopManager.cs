using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        public Button CloseButton;

        private string _currentListID = "downloaded";
        private bool _isEnabled = true;
        private HostLocalGameButton _freeplayComp;
        private ShopButtons _shopButtons;
        private Stack<MapBanner> _shopBanners = new Stack<MapBanner>();

        public void Awake()
        {
            Instance = this;
            IsOpen = true;
            ControllerManager.Instance.OpenOverlayMenu("LIShop", null);
        }

        public void Start()
        {
            _freeplayComp = gameObject.AddComponent<HostLocalGameButton>();
            _shopButtons = gameObject.GetComponent<ShopButtons>();
            _freeplayComp.NetworkMode = NetworkModes.FreePlay;
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
            while (_shopBanners.Count > 0)
            {
                MapBanner banner = _shopBanners.Pop();
                Destroy(banner.gameObject);
            }
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
                _shopBanners.Push(banner);
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
                    _shopBanners.Push(banner);
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
                    _shopBanners.Push(banner);
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
                    _shopBanners.Push(banner);
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

            AmongUsClient.Instance.TutorialMapId = (int)MapNames.Polus;
            _freeplayComp.OnClick();
        }

        public void SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
            _shopButtons.SetEnabled(isEnabled);
            foreach (MapBanner banner in _shopBanners)
            {
                banner.SetEnabled(isEnabled);
            }
            CloseButton.interactable = isEnabled;
        }

        public static void CloseShop()
        {
            if (Instance != null)
                Instance.Close();
        }
    }
}