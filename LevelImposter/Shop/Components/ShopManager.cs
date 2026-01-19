using System;
using System.Diagnostics;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.FileIO;
using LevelImposter.Networking.API;
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
    
    public static ShopManager? Instance { get; private set; }
    
    private const string CONTROLLER_OVERLAY_ID = "LIShop";
    
    /// If true, re-runs the map randomization when the shop is closed
    private bool _randomizeMapsOnClose = false;
    private ShopTab _currentTab = ShopTab.None;
    
    public void Awake()
    {
        Instance = this;
        exitButton.Value.OnClick.AddListener((Action)CloseShop);
        openMapsFolderButton.Value.OnClick.AddListener((Action)OpenMapsFolder);
        
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
        // TODO: Check if the current platform is supported (windows, linux, etc.)
        Process.Start("explorer.exe", MapFileAPI.GetDirectory());
    }

    /// <summary>
    /// Closes the shop window
    /// </summary>
    public void CloseShop()
    {
        if (_randomizeMapsOnClose)
            MapRandomizer.RandomizeMap(false);

        TransitionHelper.RunTransitionFade(
            gameObject,
            null,
            () => Destroy(gameObject));
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
                LevelImposterAPI.GetFeatured(m => OnWorkshopLoaded(m, tab), LILogger.Error);
                break;
            case ShopTab.TopWorkshopMaps:
                LevelImposterAPI.GetTop(m => OnWorkshopLoaded(m, tab), LILogger.Error);
                break;
            case ShopTab.RecentWorkshopMaps:
                LevelImposterAPI.GetRecent(m => OnWorkshopLoaded(m, tab), LILogger.Error);
                break;
            case ShopTab.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    /// <summary>
    /// Called when workshop maps are loaded.
    /// Just checks if the current tab is still the target tab before setting the maps.
    /// </summary>
    /// <param name="maps">The loaded maps</param>
    /// <param name="targetTab">The target tab</param>
    [HideFromIl2Cpp]
    private void OnWorkshopLoaded(LIMetadata[] maps, ShopTab targetTab)
    {
        if (_currentTab != targetTab)
            return;
        
        SetMaps(maps);
    }

    /// <summary>
    /// Sets the maps to display in the shop
    /// </summary>
    /// <param name="maps">The maps to display</param>
    [HideFromIl2Cpp]
    private void SetMaps(LIMetadata[] maps)
    {
        // Clear Existing Banners
        mapBannerGrid.Value.DestroyAll();
        
        // Add New Banners
        foreach (var map in maps)
        {
            // Instantiate Map Banner
            var mapBanner = Instantiate(mapBannerPrefab.Value);
            mapBanner.SetMap(map);

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