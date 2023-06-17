using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public ShopManager(IntPtr intPtr) : base(intPtr)
        {
        }

        private const float BANNER_HEIGHT = 2.0f;
        private const float BANNER_WIDTH = 2.4f;
        private const int COL_COUNT = 3;

        public static ShopManager? Instance { get; private set; }

        private Tab _currentTab = Tab.None;
        private GameObject? _overlay = null;
        private TMPro.TMP_Text? _overlayText = null;
        private Scroller? _scroller = null;
        private SpriteRenderer? _title = null;
        private ShopTabs? _tabs = null;
        private MapBanner? _bannerPrefab = null;
        private Transform? _bannerParent => _bannerPrefab?.transform.parent;
        private Stack<MapBanner>? _shopBanners = new();
        private HostLocalGameButton? _freeplayComp = null;
        private bool _shouldRegenerateFallback = false;

        public Tab CurrentTab => _currentTab;
        public SpriteRenderer? Title => _title;

        /// <summary>
        /// Closes the Map Shop
        /// </summary>
        public void Close()
        {
            ConfigAPI.Save();

            bool isInLobby = LobbyBehaviour.Instance != null;
            bool isMapLoaded = MapLoader.CurrentMap != null && !MapLoader.IsFallback;
            if (isInLobby && !isMapLoaded && _shouldRegenerateFallback)
                MapSync.RegenerateFallbackID();

            DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(gameObject, null, (Action)OnClose);
        }
        private void OnClose()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Closes the shop Instance, if one exists
        /// </summary>
        public static void CloseShop() => Instance?.Close();

        /// <summary>
        /// Clears all visible shop banners
        /// </summary>
        public void Clear()
        {
            while (_shopBanners?.Count > 0)
                Destroy(_shopBanners.Pop().gameObject);
        }

        /// <summary>
        /// Event that is called on download error
        /// </summary>
        private void OnError(string error)
        {
            LILogger.Error(error);
        }

        /// <summary>
        /// Sets the current shop to the cooresponding tab
        /// </summary>
        /// <param name="tab">The tab to set the shop to</param>
        public void SetTab(Tab tab)
        {
            // Get/Set Current Tab
            if (_currentTab == tab)
                return;
            _currentTab = tab;
            LILogger.Info($"Setting tab to {tab}");
            _tabs?.UpdateButtons();

            // Clear the shop
            Clear();

            // Switch on the tab
            Action callback = tab switch
            {
                Tab.Downloads => SetDownloadsTab,
                Tab.Featured => SetFeaturedTab,
                Tab.Top => SetTopTab,
                Tab.Recent => SetRecentTab,
                Tab.None => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
            callback.Invoke();
        }

        /// <summary>
        /// Lists all downloaded maps
        /// </summary>
        private void SetDownloadsTab()
        {
            Clear();
            string[] mapIDs = MapFileAPI.Instance?.ListIDs() ?? new string[0];
            foreach (string mapID in mapIDs)
            {
                MapFileAPI.Instance?.GetMetadata(mapID, OnDownloadsResponse);
            }
        }
        [HideFromIl2Cpp]
        private void OnDownloadsResponse(LIMetadata? metadata)
        {
            if (metadata != null && _currentTab == Tab.Downloads)
                AddBanner(metadata);
        }

        /// <summary>
        /// Lists maps in the LevelImposter API by Top
        /// </summary>
        private void SetTopTab()
        {
            LevelImposterAPI.GetTop(OnTopResponse, OnError);
        }
        [HideFromIl2Cpp]
        private void OnTopResponse(LIMetadata[] maps) => OnAPIRespose(maps, Tab.Top);

        /// <summary>
        /// Lists maps in the LevelImposter API by Recent
        /// </summary>
        private void SetRecentTab()
        {
            LevelImposterAPI.GetRecent(OnRecentResponse, OnError);
        }
        [HideFromIl2Cpp]
        private void OnRecentResponse(LIMetadata[] maps) => OnAPIRespose(maps, Tab.Recent);

        /// <summary>
        /// Lists maps in the LevelImposter API by Featured
        /// </summary>
        private void SetFeaturedTab()
        {
            LevelImposterAPI.GetFeatured(OnFeaturedResponse, OnError);
        }
        [HideFromIl2Cpp]
        private void OnFeaturedResponse(LIMetadata[] maps) => OnAPIRespose(maps, Tab.Featured);

        /// <summary>
        /// Event that is called when the API returns a response
        /// </summary>
        /// <param name="maps">List of maps of response</param>
        /// <param name="tab">Tab of response</param>
        [HideFromIl2Cpp]
        private void OnAPIRespose(LIMetadata[] maps, Tab tab)
        {
            if (_currentTab != tab)
                return;
            Clear();
            foreach (LIMetadata map in maps)
                AddBanner(map);
        }

        /// <summary>
        /// Adds a shop banner to the shop
        /// </summary>
        /// <param name="map">Map metadata to add</param>
        [HideFromIl2Cpp]
        private void AddBanner(LIMetadata map)
        {
            if (_bannerPrefab == null || _shopBanners == null)
                return;
            int bannerCount = _shopBanners.Count;

            // Instantiate Banner
            MapBanner banner = Instantiate(_bannerPrefab, _bannerParent);
            banner.gameObject.SetActive(true);
            banner.SetMap(map);
            _shopBanners.Push(banner);

            // Position Banner
            banner.transform.localPosition = new Vector3(
                (bannerCount % COL_COUNT) * BANNER_WIDTH - (COL_COUNT - 1) * (BANNER_WIDTH / 2),
                (bannerCount / COL_COUNT) * -BANNER_HEIGHT,
                0
            );

            // Set Scroll Height
            if (_scroller != null)
                _scroller.ContentYBounds.max = (bannerCount / COL_COUNT) * BANNER_HEIGHT;

        }

        /// <summary>
        /// Selects map to load by ID
        /// </summary>
        /// <param name="id">ID of the map to select</param>
        public void SelectMap(string id)
        {
            LILogger.Info($"Selecting map [{id}]");
            MapLoader.LoadMap(id, false, MapSync.SyncMapID);
            ConfigAPI.SetLastMapID(id);

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
        public void OnExternal(string id)
        {
            LILogger.Info($"Viewing map [{id}]");
            Application.OpenURL($"https://levelimposter.net/#/map/{id}");
        }

        /// <summary>
        /// Set to true to regenerate the fallback map on close
        /// </summary>
        public static void RegenerateFallbackMap()
        {
            if (Instance != null)
                Instance._shouldRegenerateFallback = true;
        }
        
        /// <summary>
        /// Toggles the overlay
        /// </summary>
        /// <param name="isEnabled"><c>true</c> if the overlay should be visible</param>
        public void SetOverlayEnabled(bool isEnabled)
        {
            _overlay?.SetActive(isEnabled);
        }
        
        /// <summary>
        /// Modifies the text of the overlay
        /// </summary>
        /// <param name="text">Text to set overlay to</param>
        public void SetOverlayText(string text)
        {
            _overlayText?.SetText(text);
        }

        public void Awake()
        {
            ControllerManager.Instance.OpenOverlayMenu("LIShop", null);
            Instance = this;

            _overlay = transform.Find("Overlay").gameObject;
            _overlayText = _overlay?.transform.Find("Text").GetComponent<TMPro.TMP_Text>();
            _scroller = transform.Find("Scroll/Scroller").GetComponent<Scroller>();
            _title = _scroller?.transform.Find("Inner/Title").GetComponent<SpriteRenderer>();
            _tabs = transform.Find("Header/Tabs").GetComponent<ShopTabs>();
            _bannerPrefab = _scroller?.transform.Find("Inner/MapBanner").GetComponent<MapBanner>();
            
        }
        public void Start()
        {
            // Set Tab
            SetTab(Tab.Downloads);

            // Prefabs
            _bannerPrefab?.gameObject.SetActive(false);

            // Freeplay Component
            _freeplayComp = gameObject.AddComponent<HostLocalGameButton>();
            _freeplayComp.NetworkMode = NetworkModes.FreePlay;

            // Starfield
            var starfield = transform.FindChild("Star Field").gameObject;
            var starGen = starfield.AddComponent<StarGen>();
            var starRenderer = starfield.GetComponent<MeshRenderer>();
            starGen.Length = 14;
            starGen.Width = 14;
            starGen.Direction = new Vector2(0, -2);
            starRenderer.material = AssetDB.GetObject("starfield")?.GetComponent<MeshRenderer>().material;
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
            
            _overlay = null;
            _overlayText = null;
            _scroller = null;
            _title = null;
            _tabs = null;
            _bannerPrefab = null;
            _freeplayComp = null;
        }

        public enum Tab
        {
            None,
            Downloads,
            Featured,
            Top,
            Recent
        }
    }
}