using System;
using System.Diagnostics;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSystem.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.FileIO;
using LevelImposter.Networking.API;
using LevelImposter.Shop.Transitions;
using LevelImposter.Lobby;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
/// Represents the different tabs in the shop
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
/// Manages the shop UI and functionality
/// </summary>
public class ShopManager(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    // Serialized Fields
    public Il2CppReferenceField<SpriteRenderer> titleRenderer;
    public Il2CppReferenceField<MapBanner> mapBannerPrefab;
    public Il2CppReferenceField<GameObjectGrid> mapBannerGrid;
    public Il2CppReferenceField<PassiveButton> exitButton;
    public Il2CppReferenceField<PassiveButton> openMapsFolderButton;
    public Il2CppReferenceField<LoadingOverlay> loadingOverlay;
    
    public static ShopManager? Instance { get; private set; }
    
    private const string CONTROLLER_OVERLAY_ID = "LIShop";
    
    public LoadingOverlay LoadingOverlay => loadingOverlay.Value;
    
    /// If true, re-runs the map randomization when the shop is closed
    private bool _randomizeMapsOnClose;
    private ShopTab _currentTab = ShopTab.None;
    private ShopTabButton[]? _shopTabButtons;
    
    public void Awake()
    {
        Instance = this;
        exitButton.Value.OnClick.AddListener((Action)CloseShop);
        openMapsFolderButton.Value.OnClick.AddListener((Action)OpenMapsFolder);

        _shopTabButtons = GetComponentsInChildren<ShopTabButton>(true);
        
        ControllerManager.Instance.OpenOverlayMenu(CONTROLLER_OVERLAY_ID, null);
    }
    public void Start()
    {
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
        ControllerManager.Instance.CloseOverlayMenu(CONTROLLER_OVERLAY_ID);
    }

    /// <summary>
    /// Opens the folder where maps are stored
    /// </summary>
    private static void OpenMapsFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = MapFileAPI.GetDirectory(),
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Closes the shop window
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
    /// Loads the specified tab in the shop
    /// </summary>
    /// <param name="tab">The tab to load</param>
    /// <param name="titleSprite">Optional title sprite to set</param>
    public void SetTab(ShopTab tab, Sprite? titleSprite = null)
    {
        // Set Title Sprite
        if (titleSprite != null)
            titleRenderer.Value.sprite = titleSprite;
        
        // Check if we're already on this tab
        if (_currentTab == tab)
            return;
        _currentTab = tab;

        UpdateTabButtonState();
        mapBannerGrid.Value.DestroyAll();

        switch (_currentTab)
        {
            case ShopTab.DownloadedMaps:
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
                    m => OnWorkshopLoaded(m, tab),
                    error => OnError(tab, error));
                break;
            case ShopTab.TopWorkshopMaps:
                LoadingOverlay.Show();
                LevelImposterAPI.GetTop(
                    m => OnWorkshopLoaded(m, tab),
                    error => OnError(tab, error));
                break;
            case ShopTab.RecentWorkshopMaps:
                LoadingOverlay.Show();
                LevelImposterAPI.GetRecent(
                    m => OnWorkshopLoaded(m, tab),
                    error => OnError(tab, error));
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
        if (_currentTab != targetTab)
            return;
        
        SetMaps(maps);
    }
    [HideFromIl2Cpp]
    private void OnError(ShopTab tab, string message)
    {
        if (_currentTab != tab)
            return;
        
        LoadingOverlay.ShowError("The impostor sabotaged comms!", message);
    }

    /// <summary>
    /// Updates the visual state of the tab buttons
    /// </summary>
    private void UpdateTabButtonState()
    {
        if (_shopTabButtons == null)
            throw new InvalidOperationException("Shop tab buttons not initialized");
        
        foreach (var tabButton in _shopTabButtons)
            tabButton.SetTabSelected(tabButton.TabType == _currentTab);
    }

    
    /// <summary>
    /// Sets the maps to display in the shop
    /// </summary>
    /// <param name="maps">The maps to display</param>
    [HideFromIl2Cpp]
    private void SetMaps(LIMetadata[] maps)
    {
        // Hide Overlays
        LoadingOverlay.Hide();
        
        // Clear Existing Banners
        mapBannerGrid.Value.DestroyAll();
        
        // Add New Banners
        var delay = 0.0f;
        foreach (var map in maps)
        {
            // Instantiate Map Banner
            var mapBanner = Instantiate(mapBannerPrefab.Value);
            mapBanner.SetMap(map);
            
            // Animate In
            MatOpacityTransition.Run(new TransitionParams<float>
            {
                TargetObject = mapBanner.gameObject,
                FromValue = 0,
                ToValue = 1,
                StartDelay = delay,
                Duration = 0.1f
            });
            delay += 0.05f;

            // Add to Grid
            mapBannerGrid.Value.AddTransform(mapBanner.transform);
        }
    }

    /// <summary>
    ///   Marks that maps should be re-randomized when the shop is closed.
    ///   Should be called whenever the user adds/removes a map from their random pool
    ///   or modifies a map's random probability.
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
        starRenderer.material = AssetDB.GetObject("starfield")?.GetComponent<MeshRenderer>().material;
    }
}