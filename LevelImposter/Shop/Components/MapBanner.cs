using System;
using System.IO;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.FileIO;
using LevelImposter.Lobby;
using LevelImposter.Networking.API;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

public class MapBanner(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    // Serialized Fields
    public Il2CppReferenceField<RandomOverlay> randomOverlay;
    public Il2CppReferenceField<SpriteRenderer> thumbnailRenderer;
    
    public Il2CppReferenceField<TextMeshPro> titleText;
    public Il2CppReferenceField<TextMeshPro> authorText;
    public Il2CppReferenceField<TextMeshPro> descriptionText;
    
    public Il2CppReferenceField<PassiveButton> downloadButton;
    public Il2CppReferenceField<PassiveButton> playButton;
    public Il2CppReferenceField<PassiveButton> trashButton;
    public Il2CppReferenceField<PassiveButton> randomButton;
    public Il2CppReferenceField<PassiveButton> externalButton;

    private LIMetadata? _currentMap;
    
    public void Awake()
    {
        playButton.Value.OnClick.AddListener((Action)OnPlayClick);
        trashButton.Value.OnClick.AddListener((Action)OnDeleteClick);
        randomButton.Value.OnClick.AddListener((Action)OnRandomClick);
        externalButton.Value.OnClick.AddListener((Action)OnExternalClick);
        downloadButton.Value.OnClick.AddListener((Action)OnDownloadClick);
    }

    private void OnRandomClick() => randomOverlay.Value.Open();
    private void OnExternalClick() => Application.OpenURL($"https://levelimposter.net/#/map/{_currentMap?.id}");
    private void OnPlayClick()
    {
        // Validate current map
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");
        
        // Check if AssetDB is initialized
        if (!AssetDB.IsInit)
            throw new InvalidOperationException("AssetDB is not initialized");
        
        // Load map from filesystem
        LILogger.Info($"Launching map {_currentMap} in freeplay");
        var map = MapFileAPI.Get(_currentMap.id);
        if (map == null)
            throw new InvalidOperationException("Failed to load map from filesystem");
        
        // Load map depending on game state
        if (!GameState.IsInLobby)
        {
            // Launch Map in Freeplay
            LaunchMapInFreeplay(map);
        }
        else if (map.mapTarget == MapTarget.Lobby)
        {
            // Load Lobby Map
            GameConfiguration.SetLobbyMap(map);
            LobbyMapBuilder.Rebuild();
            ConfigAPI.SetLobbyMapID(map.id);
            ShopManager.Instance?.CloseShop();
        }
        else
        {
            // Load LevelImposter Map
            GameConfiguration.SetMap(map);
            GameConfiguration.SetMapType(MapType.LevelImposter);
            ConfigAPI.SetLastMapID(map.id);
            ShopManager.Instance?.CloseShop();
        }
    }

    public void OnDeleteClick()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");
        
        MapFileAPI.Delete(_currentMap.id);
        UpdateButtonState();
        ShopManager.Instance?.RandomizeMapOnClose();
    }
    
    public void OnDownloadClick()
    {
        // Validate the map ID
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");
        
        // Update UI Overlay
        // ShopManager.Instance?.SetOverlayEnabled(true);
        // OnDownloadProgress(0);
        
        // Start Download
        MapFileAPI.DownloadMap(
            new Guid(_currentMap.id),
            null,
            OnMapDownloaded,
            LILogger.Error);
    }
    [HideFromIl2Cpp]
    private void OnMapDownloaded(FileStore _)
    {
        // ShopManager.Instance?.SetOverlayEnabled(false);
        ShopManager.Instance?.RandomizeMapOnClose();
        UpdateButtonState();
    }

    /// <summary>
    /// Launches the specified map in freeplay mode
    /// </summary>
    /// <param name="map">Map to launch</param>
    [HideFromIl2Cpp]
    private void LaunchMapInFreeplay(LIMap map)
    {
        GameConfiguration.SetMap(map);
        AmongUsClient.Instance.TutorialMapId = (int)MapType.LevelImposter;

        var hostGameButton = gameObject.GetOrAddComponent<HostLocalGameButton>();
        hostGameButton.NetworkMode = NetworkModes.FreePlay;
        hostGameButton.OnClick();
    }

    /// <summary>
    /// Sets map metadata for banner to display
    /// </summary>
    /// <param name="map">Map metadata to display</param>
    [HideFromIl2Cpp]
    public void SetMap(LIMetadata map)
    {
        _currentMap = map;
        
        randomOverlay.Value.SetMapID(map.id);
        
        UpdateText();
        UpdateButtonState();
        LoadThumbnail();
    }

    /// <summary>
    /// Sets the text fields based on the current map
    /// </summary>
    private void UpdateText()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");
        
        titleText.Value.text = _currentMap.name;
        if (_currentMap.IsInWorkshop)
        {
            authorText.Value.text = $"by {_currentMap.authorName}";
            descriptionText.Value.text = _currentMap.description;
        }
        else
        {
            authorText.Value.text = "(Local Map)";
            descriptionText.Value.text = "Upload this map to the workshop to play online";
        }
    }
    
    /// <summary>
    /// Shows/hides and enables/disables buttons based on the current map state
    /// </summary>
    private void UpdateButtonState()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");
        
        var isDownloaded = MapFileAPI.Exists(_currentMap.id);
        var isDownloadable = _currentMap.IsInWorkshop && _currentMap.isPublic;
        
        downloadButton.Value.gameObject.SetActive(!isDownloaded);
        randomButton.Value.gameObject.SetActive(isDownloaded);
        playButton.Value.gameObject.SetActive(isDownloaded);
        trashButton.Value.gameObject.SetActive(isDownloaded);
        
        // TODO: Fix bug where external button doesn't work on mobile
        externalButton.Value.gameObject.SetActive(_currentMap.IsInWorkshop && !LIConstants.IsMobile);
        
        randomButton.Value.SetButtonEnableState(_currentMap.IsInWorkshop);
        playButton.Value.SetButtonEnableState(_currentMap.IsInWorkshop || !GameState.IsInLobby);
        downloadButton.Value.SetButtonEnableState(isDownloadable);
        trashButton.Value.SetButtonEnableState(isDownloadable);     // <-- Prevents accidental deletion of non-public maps
    }

    /// <summary>
    /// Loads the thumbnail for the current map
    /// </summary>
    private void LoadThumbnail()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");
        if (!_currentMap.HasThumbnail)
            return;

        ThumbnailCache.Get(_currentMap.id, SetThumbnail);
    }
    private void SetThumbnail(Sprite sprite)
    {
        if (thumbnailRenderer.Value == null)
            return;     // <-- User tabbed away before thumbnail loaded
        thumbnailRenderer.Value.sprite = sprite;
    }
}