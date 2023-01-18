using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LevelImposter.Core;
using LevelImposter.DB;
using InnerNet;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public ShopManager(IntPtr intPtr) : base(intPtr)
        {
        }

        public static ShopManager? Instance { get; private set; }

        public MapBanner? MapBannerPrefab;
        public Transform? ShopParent;
        public Button? CloseButton;

        private string _currentListID = "downloaded";
        private HostLocalGameButton? _freeplayComp;
        private ShopButtons? _shopButtons;
        private TMPro.TMP_FontAsset _brookFont;
        private Stack<MapBanner> _shopBanners = new();

        /// <summary>
        /// Closes the Map Shop
        /// </summary>
        public void Close()
        {
            if (SceneManager.GetActiveScene().name == "HowToPlay")
                SceneManager.LoadScene("MainMenu");
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Clears all visible shop banners
        /// </summary>
        public void ClearList()
        {
            _currentListID = "none";
            while (_shopBanners.Count > 0)
            {
                MapBanner banner = _shopBanners.Pop();
                Destroy(banner.gameObject);
            }
            CleanAssets();
        }

        /// <summary>
        /// Mark all assets for garbage collection
        /// </summary>
        private void CleanAssets()
        {
            SpriteLoader.Instance?.ClearAll();
            WAVLoader.Instance?.ClearAll();
        }

        /// <summary>
        /// Lists all downloaded maps
        /// </summary>
        public void ListDownloaded()
        {
            if (MapBannerPrefab == null)
                return;
            if (MapFileAPI.Instance == null)
                return;
            ClearList();
            _currentListID = "downloaded";
            string[] mapIDs = MapFileAPI.Instance.ListIDs();
            foreach (string mapID in mapIDs)
            {
                MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                banner.gameObject.SetActive(true);
                MapFileAPI.Instance.GetMetadata(mapID, (metadata) =>
                {
                    if (metadata != null)
                        banner.SetMap(metadata);
                });
                _shopBanners.Push(banner);
            }
        }

        /// <summary>
        /// Lists maps in the LevelImposter API by Top
        /// </summary>
        public void ListTop()
        {
            ClearList();
            _currentListID = "top";
            LevelImposterAPI.Instance?.GetTop(OnTop);
        }

        /// <summary>
        /// Callback response for the LevelImposter API
        /// </summary>
        /// <param name="maps">Listed map response</param>
        [HideFromIl2Cpp]
        private void OnTop(LIMetadata[] maps)
        {
            if (MapBannerPrefab == null)
                return;
            if (_currentListID != "top")
                return;
            foreach (LIMetadata map in maps)
            {
                MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                banner.gameObject.SetActive(true);
                banner.SetMap(map);
                _shopBanners.Push(banner);
            }
        }

        /// <summary>
        /// Lists maps in the LevelImposter API by Recent
        /// </summary>
        public void ListRecent()
        {
            if (LevelImposterAPI.Instance == null)
                return;
            ClearList();
            _currentListID = "recent";
            LevelImposterAPI.Instance.GetRecent(OnRecent);
        }

        /// <summary>
        /// Callback response for the LevelImposter API
        /// </summary>
        /// <param name="maps">Listed map response</param>
        [HideFromIl2Cpp]
        private void OnRecent(LIMetadata[] maps)
        {
            if (MapBannerPrefab == null)
                return;
            if (_currentListID != "recent")
                return;
            foreach (LIMetadata map in maps)
            {
                MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                banner.gameObject.SetActive(true);
                banner.SetMap(map);
                _shopBanners.Push(banner);
            }
        }

        /// <summary>
        /// Lists maps in the LevelImposterAPI by Featured
        /// </summary>
        public void ListFeatured()
        {
            if (LevelImposterAPI.Instance == null)
                return;
            ClearList();
            _currentListID = "featured";
            LevelImposterAPI.Instance.GetFeatured(OnFeatured);
        }

        /// <summary>
        /// Callback response for the LevelImposter API
        /// </summary>
        /// <param name="maps">Listed map response</param>
        [HideFromIl2Cpp]
        private void OnFeatured(LIMetadata[] maps)
        {
            if (MapBannerPrefab == null)
                return;
            if (_currentListID != "featured")
                return;
            foreach (LIMetadata map in maps)
            {
                MapBanner banner = Instantiate(MapBannerPrefab, ShopParent);
                banner.gameObject.SetActive(true);
                banner.SetMap(map);
                _shopBanners.Push(banner);
            }
        }

        /// <summary>
        /// Selects map to load by ID
        /// </summary>
        /// <param name="id">ID of the map to select</param>
        public void SelectMap(string id)
        {
            LILogger.Info($"Selecting map [{id}]");
            MapLoader.LoadMap(id, MapUtils.SyncMapID);
            CloseShop();
        }

        /// <summary>
        /// Launches a map into Freeplay by ID
        /// </summary>
        /// <param name="id">ID of the map to launch</param>
        public void LaunchMap(string id)
        {
            if (!AssetDB.IsInit)
                return;
            LILogger.Info($"Launching map [{id}]");
            MapLoader.LoadMap(id, () =>
            {
                AmongUsClient.Instance.TutorialMapId = (int)MapNames.Polus;
                _freeplayComp?.OnClick();
            });
        }

        /// <summary>
        /// Enables or disables the shop buttons
        /// </summary>
        /// <param name="isEnabled">TRUE if enabled</param>
        public void SetEnabled(bool isEnabled)
        {
            _shopButtons?.SetEnabled(isEnabled);
            foreach (MapBanner banner in _shopBanners)
            {
                banner.SetEnabled(isEnabled);
            }
            if (CloseButton != null)
                CloseButton.interactable = isEnabled;
        }

        /// <summary>
        /// Closes the shop Instance, if one exists
        /// </summary>
        public static void CloseShop()
        {
            if (Instance != null)
                Instance.Close();
        }
        
        public void Awake()
        {
            ControllerManager.Instance.OpenOverlayMenu("LIShop", null);
            Instance = this;
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
                Close();
        }
        public void OnDestroy()
        {
            ControllerManager.Instance.CloseOverlayMenu("LIShop");
            Instance = null;
            MapBannerPrefab = null;
            ShopParent = null;
            CloseButton = null;
            _freeplayComp = null;
            _shopBanners.Clear();
            CleanAssets();
        }
    }
}