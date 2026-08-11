using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.Core.Models;
using LevelImposter.Core.Translations;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using LevelImposter.FileIO.API;
using LevelImposter.FileIO.Serialization;
using LevelImposter.Lobby.Sync;
using LevelImposter.Networking.API;
using LevelImposter.Shop.Transitions;
using LevelImposter.Shop.Utils;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop.Components;

/// <summary>
///     Represents the different tabs in the shop
/// </summary>
public enum ShopTab
{
    None,
    DownloadedMaps,
    DownloadedLobbyMaps,
    FeaturedWorkshopMaps,
    TopWorkshopMaps,
    RecentWorkshopMaps
}

/// <summary>
///     Manages the shop UI and functionality
/// </summary>
public class ShopManager(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private const string CONTROLLER_OVERLAY_ID = "LIShop";
    private const int MAP_BANNER_POOL_SIZE = 50;

    private readonly Dictionary<ShopTab, LIMetadata[]> _cachedData = new();
    private readonly MapBannerPool _mapBannerPool = new();

    /// If true, re-runs the map randomization when the shop is closed
    private bool _randomizeMapsOnClose;

    private ShopTabButton[]? _shopTabButtons;
    public Il2CppReferenceField<PassiveButton> exitButton;
    public Il2CppReferenceField<LoadingOverlay> loadingOverlay;
    public Il2CppReferenceField<GameObjectGrid> mapBannerGrid;
    public Il2CppReferenceField<MapBanner> mapBannerPrefab;
    public Il2CppReferenceField<PassiveButton> openMapsFolderButton;
    public Il2CppReferenceField<TMP_Text> titleText;
    public ShopTab CurrentTab { get; private set; } = ShopTab.None;

    public static ShopManager? Instance { get; private set; }

    public LoadingOverlay LoadingOverlay => loadingOverlay.Value;

    public void Awake()
    {
        Instance = this;
        exitButton.Value.OnClick.AddListener((Action)CloseShop);
        openMapsFolderButton.Value.OnClick.AddListener((Action)MapFileAPI.OpenInExplorer);

        _shopTabButtons = GetComponentsInChildren<ShopTabButton>(true);

        gameObject.AddComponent<MapsFolderWatcher>();

        ControllerManager.Instance.OpenOverlayMenu(CONTROLLER_OVERLAY_ID, null);
    }

    public void Start()
    {
        _mapBannerPool.Initialize(
            mapBannerPrefab.Value,
            MAP_BANNER_POOL_SIZE);

        SetTab(ShopTab.DownloadedMaps);
        AddStarField();
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            CloseShop();
    }

    public void OnDestroy()
    {
        Instance = null;
        ControllerManager.Instance.CloseOverlayMenu(CONTROLLER_OVERLAY_ID);
    }

    /// <summary>
    ///     Closes the shop window
    /// </summary>
    public void CloseShop()
    {
        if (LoadingOverlay.PreventClose)
            return;

        if (_randomizeMapsOnClose && GameConfiguration.HideMapName)
            MapRandomizer.RandomizeMap(false);

        DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(
            gameObject,
            null,
            (Action)(() => Destroy(gameObject)));
    }

    /// <summary>
    ///     Loads the specified tab in the shop
    /// </summary>
    /// <param name="tab">The tab to load</param>
    public void SetTab(ShopTab tab)
    {
        // Set Title Sprite
        titleText.Value.text = tab switch
        {
            ShopTab.DownloadedMaps => Translation.Get("shop.tabs.downloaded_maps"),
            ShopTab.DownloadedLobbyMaps => Translation.Get("shop.tabs.lobby_maps"),
            ShopTab.FeaturedWorkshopMaps => Translation.Get("shop.tabs.featured_maps"),
            ShopTab.TopWorkshopMaps => Translation.Get("shop.tabs.top_maps"),
            ShopTab.RecentWorkshopMaps => Translation.Get("shop.tabs.recent_maps"),
            ShopTab.None => "",
            _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, null)
        };

        // Check if we're already on this tab
        if (CurrentTab == tab)
            return;
        CurrentTab = tab;

        UpdateTabButtonState();
        RefreshTab();
    }

    /// <summary>
    ///     Refreshes the content on the current tab
    /// </summary>
    public void RefreshTab()
    {
        // Hide all existing map banners
        HideAllMaps();

        // Check if this tab is cached
        if (_cachedData.TryGetValue(CurrentTab, out var cachedData))
        {
            SetMaps(cachedData);
            return;
        }

        // Save a copy of current tab
        // (Prevents content change if the tab changes mid-load)
        var targetTab = CurrentTab;

        switch (CurrentTab)
        {
            case ShopTab.DownloadedMaps:
                LIDeserializerLegacy.ConvertAllLegacyMaps();
                var maps = MapFileAPI.GetAllMetadata()
                    .Where(m => m.mapTarget != MapTarget.Lobby)
                    .ToArray();
                SetMaps(maps);
                break;
            case ShopTab.DownloadedLobbyMaps:
                var lobbyMaps = MapFileAPI.GetAllMetadata()
                    .Where(m => m.mapTarget == MapTarget.Lobby)
                    .ToArray();
                SetMaps(lobbyMaps);
                break;
            case ShopTab.FeaturedWorkshopMaps:
                LoadingOverlay.Show();
                LevelImposterAPI.GetFeatured(
                    m => OnWorkshopLoaded(m, targetTab),
                    error => OnError(targetTab, error));
                break;
            case ShopTab.TopWorkshopMaps:
                LoadingOverlay.Show();
                LevelImposterAPI.GetTop(
                    m => OnWorkshopLoaded(m, targetTab),
                    error => OnError(targetTab, error));
                break;
            case ShopTab.RecentWorkshopMaps:
                LoadingOverlay.Show();
                LevelImposterAPI.GetRecent(
                    m => OnWorkshopLoaded(m, targetTab),
                    error => OnError(targetTab, error));
                break;
            case ShopTab.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [HideFromIl2Cpp]
    private void OnWorkshopLoaded(LIMetadata[] maps, ShopTab targetTab)
    {
        // Cache response
        _cachedData[targetTab] = maps;

        // Check if the user is still on this tab
        if (CurrentTab == targetTab)
            SetMaps(maps);
    }

    [HideFromIl2Cpp]
    private void OnError(ShopTab tab, string message)
    {
        if (CurrentTab != tab)
            return;

        LoadingOverlay.ShowError("The impostor sabotaged comms!", message);
        SetMaps([]);
    }

    /// <summary>
    ///     Updates the visual state of the tab buttons
    /// </summary>
    private void UpdateTabButtonState()
    {
        if (_shopTabButtons == null)
            throw new InvalidOperationException("Shop tab buttons not initialized");

        foreach (var tabButton in _shopTabButtons)
            tabButton.SetTabSelected(tabButton.TabType == CurrentTab);
    }

    private void HideAllMaps()
    {
        _mapBannerPool.ReturnAll();
        mapBannerGrid.Value.Reset();
    }

    /// <summary>
    ///     Sets the maps to display in the shop
    /// </summary>
    /// <param name="maps">The maps to display</param>
    [HideFromIl2Cpp]
    private void SetMaps(LIMetadata[] maps)
    {
        // Hide Overlays
        LoadingOverlay.Hide();
        HideAllMaps();

        // Add New Banners
        var delay = 0.0f;
        for (var i = 0; i < maps.Length; i++)
        {
            // Get Map Banner from Pool
            var poolItem = _mapBannerPool.Get();
            poolItem.MapBanner.SetMap(maps[i]);
            mapBannerGrid.Value.AddTransform(poolItem.MapBanner.transform);

            // Animate In
            poolItem.TransitionCoroutine = MatOpacityTransition.Run(new TransitionParams<float>
            {
                TargetObject = poolItem.MapBanner.gameObject,
                FromValue = 0,
                ToValue = 1,
                StartDelay = delay,
                Duration = 0.1f
            });
            delay += 0.05f;
        }
    }

    /// <summary>
    ///     Marks that maps should be re-randomized when the shop is closed.
    ///     Should be called whenever the user adds/removes a map from their random pool
    ///     or modifies a map's random probability.
    /// </summary>
    public void RandomizeMapOnClose()
    {
        _randomizeMapsOnClose = true;
    }

    private void AddStarField()
    {
        var starfield = transform.FindChild("Star Field").gameObject;
        var starGen = starfield.AddComponent<StarGen>();
        var starRenderer = starfield.GetComponent<MeshRenderer>();
        starGen.Length = 14;
        starGen.Width = 14;
        starGen.Direction = new Vector2(0, -2);
        starRenderer.material = PrefabDB.GetObject("starfield")?.GetComponent<MeshRenderer>().material;
    }
}