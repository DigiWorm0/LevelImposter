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

        public MapBanner? MapBannerPrefab = null;
        public Transform? ShopParent = null;
        public Button? CloseButton = null;

        private string _currentListID = "downloaded";
        private HostLocalGameButton? _freeplayComp = null;
        private ShopButtons? _shopButtons = null;
        private Stack<MapBanner> _shopBanners = new();
        private bool _shouldRegenerateFallback = false;

        /// <summary>
        /// Closes the Map Shop
        /// </summary>
        public void Close()
        {
            ConfigAPI.Instance?.Save();

            bool isInLobby = LobbyBehaviour.Instance != null;
            bool isMapLoaded = MapLoader.CurrentMap != null && !MapLoader.IsFallback;
            if (isInLobby && !isMapLoaded && _shouldRegenerateFallback)
                MapSync.RegenerateFallbackID();

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
        }

        /// <summary>
        /// Event that is called on download error
        /// </summary>
        private void OnError(string error)
        {
            LILogger.Error(error);
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
            LILogger.Info("Listing downloaded maps");
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
            LILogger.Info("Listing top maps");
            ClearList();
            _currentListID = "top";
            LevelImposterAPI.Instance?.GetTop(OnTop, OnError);
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
            LILogger.Info("Listing recent maps");
            ClearList();
            _currentListID = "recent";
            LevelImposterAPI.Instance.GetRecent(OnRecent, OnError);
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
            LILogger.Info("Listing featured maps");
            ClearList();
            _currentListID = "featured";
            LevelImposterAPI.Instance.GetFeatured(OnFeatured, OnError);
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
            MapLoader.LoadMap(id, false, MapSync.SyncMapID);
            ConfigAPI.Instance?.SetLastMapID(id);

            _shouldRegenerateFallback = false;
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
            RandomizerSync.SyncRandomSeed();
            GCHandler.Clean();
            MapLoader.LoadMap(id, false, () =>
            {
                AmongUsClient.Instance.TutorialMapId = (int)MapType.LevelImposter;
                _freeplayComp?.OnClick();
            });
        }

        /// <summary>
        /// Launches a map in browser
        /// </summary>
        /// <param name="id">ID of the map to view. Must be in LevelImposter API</param>
        public void ViewMap(string id)
        {
            LILogger.Info($"Viewing map [{id}]");
            Application.OpenURL($"https://levelimposter.net/#/map/{id}");
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
        /// Closes all map banner popups
        /// </summary>
        public void CloseAllPopups()
        {
            foreach (MapBanner banner in _shopBanners)
            {
                banner.CloseAllPopups();
            }
        }

        /// <summary>
        /// Closes the shop Instance, if one exists
        /// </summary>
        public static void CloseShop()
        {
            if (Instance != null)
                Instance.Close();
        }

        /// <summary>
        /// Set to true to regenerate the fallback map on close
        /// </summary>
        /// <param name="shouldRegenerateFallback">True iff the fallback map should be reset</param>
        public static void RegenerateFallback(bool shouldRegenerateFallback)
        {
            if (Instance != null)
                Instance._shouldRegenerateFallback = shouldRegenerateFallback;
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
        }
    }
}