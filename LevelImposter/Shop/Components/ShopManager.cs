using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using LevelImposter.DB;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

public class ShopManager(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    /// <summary>
    ///     Enum of tabs in the shop
    /// </summary>
    public enum Tab
    {
        None,
        Downloads,
        Featured,
        Top,
        Recent
    }

    private const string SHOP_NAME = "LIShop";
    private const float BANNER_HEIGHT = 2.0f;
    private const float BANNER_WIDTH = 2.4f;
    private const int COL_COUNT = 3;
    private readonly Stack<MapBanner>? _shopBanners = new();
    private MapBanner? _bannerPrefab;

    private HostLocalGameButton? _freeplayComp;
    private GameObject? _overlay;
    private SpriteRenderer? _overlayBackground;
    private TMP_Text? _overlayText;
    private Scroller? _scroller;
    private bool _shouldRegenerateFallback;
    private ShopTabs? _tabs;

    public static ShopManager? Instance { get; private set; }
    private Transform? _bannerParent => _bannerPrefab?.transform.parent;

    public Tab CurrentTab { get; private set; } = Tab.None;

    public SpriteRenderer? Title { get; private set; }

    public void Awake()
    {
        ControllerManager.Instance.OpenOverlayMenu(SHOP_NAME, null);
        Instance = this;

        _overlay = transform.Find("Overlay").gameObject;
        _overlayBackground = _overlay?.GetComponent<SpriteRenderer>();
        _overlayText = _overlay?.transform.Find("Text").GetComponent<TMP_Text>();
        _scroller = transform.Find("Scroll/Scroller").GetComponent<Scroller>();
        Title = _scroller?.transform.Find("Inner/Title").GetComponent<SpriteRenderer>();
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

        // AssetDB
        if (!AssetDB.IsInit)
            StartCoroutine(CoWaitForAssetDB().WrapToIl2Cpp());
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            Close();
    }

    public void OnDestroy()
    {
        ControllerManager.Instance.CloseOverlayMenu(SHOP_NAME);
        Instance = null;

        _overlay = null;
        _overlayText = null;
        _scroller = null;
        Title = null;
        _tabs = null;
        _bannerPrefab = null;
        _freeplayComp = null;
    }

    /// <summary>
    ///     Closes the Map Shop
    /// </summary>
    public void Close()
    {
        ConfigAPI.Save();

        if (GameState.IsInLobby && !GameState.IsCustomMapLoaded && _shouldRegenerateFallback)
            MapSync.RegenerateFallbackID();

        DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(gameObject, null, (Action)OnClose);
    }

    private void OnClose()
    {
        Destroy(gameObject);
    }

    /// <summary>
    ///     Closes the shop Instance, if one exists
    /// </summary>
    public static void CloseShop()
    {
        Instance?.Close();
    }

    /// <summary>
    ///     Clears all visible shop banners
    /// </summary>
    public void Clear()
    {
        _scroller?.ScrollToTop();
        while (_shopBanners?.Count > 0)
            Destroy(_shopBanners.Pop().gameObject);
    }

    /// <summary>
    ///     Event that is called on download error
    /// </summary>
    private void OnError(string error)
    {
        LILogger.Error(error);
    }

    /// <summary>
    ///     Sets the current shop to the cooresponding tab
    /// </summary>
    /// <param name="tab">The tab to set the shop to</param>
    public void SetTab(Tab tab)
    {
        // Get/Set Current Tab
        if (CurrentTab == tab)
            return;
        CurrentTab = tab;
        LILogger.Info($"Setting tab to {tab}");
        _tabs?.UpdateButtons();

        // Clear the shop
        Clear();

        // Show Loading Spinner
        SetOverlayText("Retrieving Maps...");
        SetOverlayEnabled(true, false);

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
    ///     Lists all downloaded maps
    /// </summary>
    private void SetDownloadsTab()
    {
        StartCoroutine(CoSetDownloadsTab().WrapToIl2Cpp());
    }

    [HideFromIl2Cpp]
    private IEnumerator CoSetDownloadsTab()
    {
        {
            yield return LegacyConverter.ConvertAllMaps().WrapToIl2Cpp();
            yield return null;
            Clear();
            var mapIDs = MapFileAPI.ListIDs() ?? new string[0];
            foreach (var mapID in mapIDs)
            {
                var metadata = MapFileAPI.GetMetadata(mapID);
                if (metadata != null)
                    AddBanner(metadata);
                yield return null;
            }

            // Hide Loading Spinner
            SetOverlayEnabled(false);
        }
    }

    /// <summary>
    ///     Lists maps in the LevelImposter API by Top
    /// </summary>
    private void SetTopTab()
    {
        LevelImposterAPI.GetTop(OnTopResponse, OnError);
    }

    [HideFromIl2Cpp]
    private void OnTopResponse(LIMetadata[] maps)
    {
        OnAPIRespose(maps, Tab.Top);
    }

    /// <summary>
    ///     Lists maps in the LevelImposter API by Recent
    /// </summary>
    private void SetRecentTab()
    {
        LevelImposterAPI.GetRecent(OnRecentResponse, OnError);
    }

    [HideFromIl2Cpp]
    private void OnRecentResponse(LIMetadata[] maps)
    {
        OnAPIRespose(maps, Tab.Recent);
    }

    /// <summary>
    ///     Lists maps in the LevelImposter API by Featured
    /// </summary>
    private void SetFeaturedTab()
    {
        LevelImposterAPI.GetFeatured(OnFeaturedResponse, OnError);
    }

    [HideFromIl2Cpp]
    private void OnFeaturedResponse(LIMetadata[] maps)
    {
        OnAPIRespose(maps, Tab.Featured);
    }

    /// <summary>
    ///     Event that is called when the API returns a response
    /// </summary>
    /// <param name="maps">List of maps of response</param>
    /// <param name="tab">Tab of response</param>
    [HideFromIl2Cpp]
    private void OnAPIRespose(LIMetadata[] maps, Tab tab)
    {
        if (CurrentTab != tab)
            return;
        Clear();
        foreach (var map in maps)
            AddBanner(map);

        // Hide Loading Spinner
        SetOverlayEnabled(false);
    }

    /// <summary>
    ///     Adds a shop banner to the shop
    /// </summary>
    /// <param name="map">Map metadata to add</param>
    [HideFromIl2Cpp]
    private void AddBanner(LIMetadata map)
    {
        if (_bannerPrefab == null || _shopBanners == null)
            return;
        var bannerCount = _shopBanners.Count;

        // Instantiate Banner
        var banner = Instantiate(_bannerPrefab, _bannerParent);
        banner.gameObject.SetActive(true);
        banner.SetMap(map);
        _shopBanners.Push(banner);

        // Position Banner
        banner.transform.localPosition = new Vector3(
            bannerCount % COL_COUNT * BANNER_WIDTH - (COL_COUNT - 1) * (BANNER_WIDTH / 2),
            bannerCount / COL_COUNT * -BANNER_HEIGHT,
            0
        );

        // Set Scroll Height
        if (_scroller != null)
            _scroller.ContentYBounds.max = bannerCount / COL_COUNT * BANNER_HEIGHT;
    }

    /// <summary>
    ///     Selects map to load by ID
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
    ///     Launches a map into Freeplay by ID
    /// </summary>
    /// <param name="id">ID of the map to launch</param>
    public void LaunchMap(string id)
    {
        if (!AssetDB.IsInit)
            return;
        LILogger.Info($"Launching map [{id}]");
        RandomizerSync.SyncRandomSeed();
        if (LIConstants.FREEPLAY_FLUSH_CACHE)
            GCHandler.Clean();
        MapLoader.LoadMap(id, false, () =>
        {
            AmongUsClient.Instance.TutorialMapId = (int)MapType.LevelImposter;
            _freeplayComp?.OnClick();
        });
    }

    /// <summary>
    ///     Launches a map in browser
    /// </summary>
    /// <param name="id">ID of the map to view. Must be in LevelImposter API</param>
    public void OnExternal(string id)
    {
        LILogger.Info($"Viewing map [{id}]");
        Application.OpenURL($"https://levelimposter.net/#/map/{id}");
    }

    /// <summary>
    ///     Set to true to regenerate the fallback map on close
    /// </summary>
    public static void RegenerateFallbackMap()
    {
        if (Instance != null)
            Instance._shouldRegenerateFallback = true;
    }

    /// <summary>
    ///     Toggles the overlay
    /// </summary>
    /// <param name="isEnabled"><c>true</c> if the overlay should be visible</param>
    /// <param name="isBackgroundEnabled"><c>true</c> if the overlay background should be visible</param>
    public void SetOverlayEnabled(bool isEnabled, bool isBackgroundEnabled = true)
    {
        _overlay?.SetActive(isEnabled);
        if (_overlayBackground != null)
            _overlayBackground.enabled = isBackgroundEnabled;
    }

    /// <summary>
    ///     Modifies the text of the overlay
    /// </summary>
    /// <param name="text">Text to set overlay to</param>
    public void SetOverlayText(string text)
    {
        _overlayText?.SetText(text);
    }

    /// <summary>
    ///     Coroutine to wait for AssetDB to be initialized
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoWaitForAssetDB()
    {
        {
            string? currentStatusText = null;
            SetOverlayEnabled(true);
            while (!AssetDB.IsInit)
            {
                if (currentStatusText != AssetDB.Instance?.Status)
                    SetOverlayText($"<b>Loading AssetDB...</b>\n{AssetDB.Instance?.Status}");
                currentStatusText = AssetDB.Instance?.Status;
                yield return null;
            }

            SetOverlayEnabled(false);
        }
    }
}