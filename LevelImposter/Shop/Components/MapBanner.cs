using System;
using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using InnerNet;
using LevelImposter.Core.Models;
using LevelImposter.Core.Translations;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using LevelImposter.FileIO.API;
using LevelImposter.FileIO.Cache;
using LevelImposter.FileIO.DataStores;
using LevelImposter.Lobby.Builders;
using LevelImposter.Lobby.Sync;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop.Components;

public class MapBanner(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private LIMetadata? _currentMap;
    private Sprite? _defaultThumbnail;

    public Il2CppReferenceField<TextMeshPro> authorText = null!;
    public Il2CppReferenceField<TextMeshPro> descriptionText = null!;
    public Il2CppReferenceField<PassiveButton> downloadButton = null!;
    public Il2CppReferenceField<PassiveButton> externalButton = null!;
    public Il2CppReferenceField<PassiveButton> playButton = null!;
    public Il2CppReferenceField<PassiveButton> randomButton = null!;
    public Il2CppReferenceField<RandomOverlay> randomOverlay = null!;
    public Il2CppReferenceField<SpriteRenderer> thumbnailRenderer = null!;
    public Il2CppReferenceField<TextMeshPro> titleText = null!;
    public Il2CppReferenceField<PassiveButton> trashButton = null!;

    public void Awake()
    {
        playButton.Value.OnClick.AddListener((Action)OnPlayClick);
        trashButton.Value.OnClick.AddListener((Action)OnDeleteClick);
        randomButton.Value.OnClick.AddListener((Action)OnRandomClick);
        externalButton.Value.OnClick.AddListener((Action)OnExternalClick);
        downloadButton.Value.OnClick.AddListener((Action)OnDownloadClick);

        _defaultThumbnail = thumbnailRenderer.Value.sprite;
    }

    private void OnRandomClick()
    {
        randomOverlay.Value.Open();
    }

    private void OnExternalClick()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = $"https://levelimposter.net/#/map/{_currentMap?.id}",
            UseShellExecute = true
        });
    }

    private void OnPlayClick()
    {
        // Validate current map
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");

        // Check if AssetDB is initialized
        if (!PrefabDB.IsInit)
            throw new InvalidOperationException("AssetDB is not initialized");

        // Load map from filesystem
        LILogger.Info($"Launching map {_currentMap}");
        var map = MapFileAPI.Get(_currentMap.id);
        if (map == null)
            throw new InvalidOperationException("Failed to load map from filesystem");

        // Load map depending on game state
        if (!GameState.IsInLobby)
        {
            // Launch Map in Freeplay
            LaunchMapInFreeplay(map);
        }
        else if (map.MapTarget == MapTarget.Lobby)
        {
            var isLobbyChanged = GameConfiguration.CurrentLobbyMap?.id != map.id;

            // Load Lobby Map
            GameConfiguration.SetLobbyMap(map);
            GameConfigurationSync.SendGameConfigurationRPC();
            if (isLobbyChanged)
                LobbyMapBuilder.Rebuild();

            ConfigAPI.SetLobbyMapID(map.id);
            ShopManager.Instance?.CloseShop();
        }
        else
        {
            // Load LevelImposter Map
            GameConfiguration.SetMap(map);
            GameConfiguration.SetMapType(MapType.LevelImposter);
            GameConfigurationSync.SendGameConfigurationRPC();

            ConfigAPI.SetLastMapID(map.id);
            ShopManager.Instance?.CloseShop();
        }
    }

    public void OnDeleteClick()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");

        MapsFolderWatcher.IgnoreChanges();
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
        ShopManager.Instance?.LoadingOverlay.Show(true, true);
        ShopManager.Instance?.LoadingOverlay.SetText(
            Translation.Get("shop.downloader.title", _currentMap.name),
            Translation.Get("shop.downloader.looking_for_url"));

        // Start Download
        MapFileAPI.DownloadMap(
            new Guid(_currentMap.id),
            OnMapDownloadProgress,
            OnMapDownloaded,
            OnMapDownloadError);
    }

    [HideFromIl2Cpp]
    private void OnMapDownloaded(FileStore _)
    {
        if (this == null)
            return;

        MapsFolderWatcher.IgnoreChanges();
        ShopManager.Instance?.LoadingOverlay.Hide();
        ShopManager.Instance?.RandomizeMapOnClose();
        UpdateButtonState();
    }

    private void OnMapDownloadProgress(float percent)
    {
        if (this == null)
            return;

        ShopManager.Instance?.LoadingOverlay.SetText(
            Translation.Get("shop.downloader.title", _currentMap?.name ?? "???"),
            $"{Mathf.RoundToInt(percent * 100)}%");

        ShopManager.Instance?.LoadingOverlay.SetProgress(percent);
    }

    private void OnMapDownloadError(string error)
    {
        if (this == null)
            return;

        ShopManager.Instance?.LoadingOverlay.ShowError(
            Translation.Get("shop.downloader.error"),
            error);
    }

    /// <summary>
    ///     Launches the specified map in freeplay mode
    /// </summary>
    /// <param name="map">Map to launch</param>
    [HideFromIl2Cpp]
    private void LaunchMapInFreeplay(LIMap map)
    {
        // Stop background sounds
        SoundManager.Instance.StopAllSound();

        // Load map to GameConfiguration
        var isLobby = map.MapTarget == MapTarget.Lobby;
        if (isLobby)
            GameConfiguration.SetLobbyMap(map);
        else
            GameConfiguration.SetMap(map);

        // Set game options
        AmongUsClient.Instance.MainMenuScene = "MainMenu";
        AmongUsClient.Instance.OnlineScene = isLobby ? "OnlineGame" : "Tutorial";
        AmongUsClient.Instance.NetworkMode = NetworkModes.FreePlay;
        AmongUsClient.Instance.TutorialMapId = (int)MapType.LevelImposter;

        // Start local server
        DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
        AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023, false);


        // Connect to server as client
        AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient, null);
    }

    /// <summary>
    ///     Sets map metadata for banner to display
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
    ///     Sets the text fields based on the current map
    /// </summary>
    private void UpdateText()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");

        titleText.Value.text = _currentMap.name;
        if (_currentMap.IsInWorkshop)
        {
            authorText.Value.text = Translation.Get("shop.map.author", _currentMap.authorName);
            descriptionText.Value.text = _currentMap.description;
        }
        else
        {
            authorText.Value.text = Translation.Get("shop.map.localmap");
            descriptionText.Value.text = Translation.Get("shop.map.localmap.description");
        }
    }

    /// <summary>
    ///     Shows/hides and enables/disables buttons based on the current map state
    /// </summary>
    private void UpdateButtonState()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");

        var isDownloaded = MapFileAPI.Exists(_currentMap.id);
        var isDownloadable = _currentMap.IsInWorkshop && _currentMap.isPublic;
        var isGameMap = _currentMap.MapTarget == MapTarget.Game;

        downloadButton.Value.gameObject.SetActive(!isDownloaded);
        randomButton.Value.gameObject.SetActive(isDownloaded);
        playButton.Value.gameObject.SetActive(isDownloaded);
        trashButton.Value.gameObject.SetActive(isDownloaded);

        // TODO: Fix bug where external button doesn't work on mobile
        externalButton.Value.gameObject.SetActive(_currentMap.IsInWorkshop && !GameState.IsMobile);

        randomButton.Value.SetButtonEnableState(_currentMap.IsInWorkshop && isGameMap);
        playButton.Value.SetButtonEnableState(_currentMap.IsInWorkshop || !GameState.IsInLobby);
        downloadButton.Value.SetButtonEnableState(isDownloadable);
        trashButton.Value.SetButtonEnableState(isDownloadable); // <-- Prevents accidental deletion of non-public maps
    }

    /// <summary>
    ///     Loads the thumbnail for the current map
    /// </summary>
    private void LoadThumbnail()
    {
        if (_currentMap == null)
            throw new InvalidOperationException("Current map is null");

        // Reset thumbnail
        SetThumbnail(_defaultThumbnail);

        // Load thumbnail in the background
        var mapID = _currentMap.id;
        if (_currentMap.HasThumbnail)
            ThumbnailCache.Get(_currentMap.id, sprite => OnThumbnailLoad(sprite, mapID));
    }

    private void OnThumbnailLoad(Sprite? sprite, string mapID)
    {
        // Check if the map changed in the time it took for the thumbnail load
        if (mapID != _currentMap?.id)
            return;

        SetThumbnail(sprite);
    }

    private void SetThumbnail(Sprite? sprite)
    {
        // Check if this is destroyed
        if (this == null)
            return;

        thumbnailRenderer.Value.sprite = sprite ?? _defaultThumbnail;
    }
}