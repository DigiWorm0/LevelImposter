using System;
using System.IO;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using LevelImposter.FileIO;
using LevelImposter.Networking.API;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

public class MapBanner(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private TMP_Text? _author;

    private LIMetadata? _currentMap;
    private TMP_Text? _description;
    private PassiveButton? _downloadButton;
    private PassiveButton? _externalButton;
    private PassiveButton? _playButton;
    private PassiveButton? _randomButton;
    private RandomOverlay? _randomOverlay;
    private PassiveButton? _remixButton;
    private SpriteRenderer? _thumbnail;
    private TMP_Text? _title;
    private PassiveButton? _trashButton;

    public void Awake()
    {
        _thumbnail = transform.Find("Thumbnail")?.GetComponent<SpriteRenderer>();
        _title = transform.Find("Title")?.GetComponent<TMP_Text>();
        _author = transform.Find("Author")?.GetComponent<TMP_Text>();
        _description = transform.Find("Description")?.GetComponent<TMP_Text>();
        _downloadButton = transform.Find("DownloadButton")?.GetComponent<PassiveButton>();
        _playButton = transform.Find("PlayButton")?.GetComponent<PassiveButton>();
        _randomButton = transform.Find("RandomButton")?.GetComponent<PassiveButton>();
        _trashButton = transform.Find("TrashButton")?.GetComponent<PassiveButton>();
        _remixButton = transform.Find("RemixButton")?.GetComponent<PassiveButton>();
        _externalButton = transform.Find("ExternalButton")?.GetComponent<PassiveButton>();
        _randomOverlay = transform.GetComponentInChildren<RandomOverlay>(true);
    }

    public void Start()
    {
        // Buttons
        _downloadButton?.OnClick.AddListener((Action)OnDownloadClick);
        _playButton?.OnClick.AddListener((Action)OnPlayClick);
        _randomButton?.OnClick.AddListener((Action)OnRandomClick);
        _trashButton?.OnClick.AddListener((Action)OnDeleteClick);
        _externalButton?.OnClick.AddListener((Action)OnExternalClick);

        UpdateButtons();
    }

    public void OnDestroy()
    {
        _currentMap = null;
        _thumbnail = null;
        _title = null;
        _author = null;
        _description = null;
        _downloadButton = null;
        _playButton = null;
        _randomButton = null;
        _trashButton = null;
        _remixButton = null;
        _externalButton = null;
        _randomOverlay = null;
    }

    /// <summary>
    ///     Sets map metadata for banner to display
    /// </summary>
    /// <param name="map">Map metadata to display</param>
    [HideFromIl2Cpp]
    public void SetMap(LIMetadata map)
    {
        _currentMap = map;
        _title?.SetText(map.name);
        _author?.SetText($"by {map.authorName}");
        _description?.SetText(map.description);
        UpdateButtons();
        UpdateThumbnail();
    }

    /// <summary>
    ///     Updates the interactable state of all buttons
    /// </summary>
    private void UpdateButtons()
    {
        var isLoaded = _currentMap != null;
        var isDownloaded = MapFileAPI.Exists(_currentMap?.id);
        var isOnline = !string.IsNullOrEmpty(_currentMap?.authorID) && Guid.TryParse(_currentMap.id, out _);
        var isPublic = _currentMap?.isPublic ?? false;
        var isRemix = _currentMap?.remixOf != null;
        var isInLobby = GameState.IsInLobby;

        _playButton?.SetButtonEnableState(isLoaded && isDownloaded && (isOnline || !isInLobby));
        _randomButton?.SetButtonEnableState(isLoaded && isDownloaded && isOnline);
        _trashButton?.SetButtonEnableState(isLoaded && isDownloaded && isPublic);
        _downloadButton?.SetButtonEnableState(isLoaded && !isDownloaded && isPublic);
        _remixButton?.SetButtonEnableState(isLoaded && isRemix);
        _externalButton?.gameObject.SetActive(isLoaded && isOnline && !LIConstants.IsMobile);
    }

    /// <summary>
    ///     Event that is called when the download button is pressed
    /// </summary>
    public void OnDownloadClick()
    {
        // Validate the map ID
        if (_currentMap?.id == null)
            throw new Exception("Current map is null or has no ID");
        
        // Update UI Overlay
        ShopManager.Instance?.SetOverlayEnabled(true);
        OnDownloadProgress(0);
        
        // Start Download
        MapFileAPI.DownloadMap(
            new Guid(_currentMap.id),
            OnDownloadProgress,
            OnDownload,
            OnError);
    }

    /// <summary>
    ///     Event that is called when the <c>LIMap</c> is downloaded
    /// </summary>
    [HideFromIl2Cpp]
    private void OnDownload(FileStore _)
    {
        ShopManager.Instance?.SetOverlayEnabled(false);
        ShopManager.RegenerateFallbackMap();
        UpdateButtons();
    }

    /// <summary>
    ///     Callback on download progress
    /// </summary>
    /// <param name="progress">Value from 0 to 1</param>
    private void OnDownloadProgress(float progress)
    {
        var progressPercent = (int)(progress * 100);
        ShopManager.Instance?.SetOverlayText(
            $"<b>Downloading {_currentMap?.name ?? "map"}...</b>\n{progressPercent}%");
    }

    /// <summary>
    ///     Event that is called when there is a download error
    /// </summary>
    /// <param name="error">Error info</param>
    [HideFromIl2Cpp]
    private void OnError(string error)
    {
        LILogger.Error(error);
        ShopManager.Instance?.SetOverlayEnabled(false);

        if (GameState.IsInLobby)
            DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage(error);
    }

    /// <summary>
    ///     Event that is called when the play button is pressed
    /// </summary>
    public void OnPlayClick()
    {
        if (_currentMap == null)
            return;
        
        // Launch or select map
        if (GameState.IsInLobby)
            ShopManager.Instance?.SelectMap(_currentMap.id);
        else
            ShopManager.Instance?.LaunchMap(_currentMap.id);
    }

    /// <summary>
    ///     Opens the random overlay
    /// </summary>
    public void OnRandomClick()
    {
        if (_currentMap == null)
            return;
        _randomOverlay?.Open(_currentMap.id);
    }

    /// <summary>
    ///     Event that is called when the delete button is pressed
    /// </summary>
    public void OnDeleteClick()
    {
        if (_currentMap == null)
            return;
        MapFileAPI.Delete(_currentMap?.id ?? "");
        UpdateButtons();
        ShopManager.RegenerateFallbackMap();
    }

    /// <summary>
    ///     Event that is called when the external button is pressed
    /// </summary>
    public void OnExternalClick()
    {
        if (_currentMap == null)
            return;
        ShopManager.Instance?.OnExternal(_currentMap.id);
    }

    /// <summary>
    ///     Updates the map banner's active thumbnail
    /// </summary>
    private void UpdateThumbnail()
    {
        if (_currentMap == null)
            return;
        if (!_currentMap.HasThumbnail)
            return;

        ThumbnailCache.Get(_currentMap.id, SetThumbnail);
    }

    private void SetThumbnail(Sprite sprite)
    {
        if (_thumbnail != null)
            _thumbnail.sprite = sprite;
    }
}