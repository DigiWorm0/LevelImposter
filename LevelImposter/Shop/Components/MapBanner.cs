using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LevelImposter.Core;
using Il2CppInterop.Runtime.Attributes;
using Discord;

namespace LevelImposter.Shop
{
    public class MapBanner : MonoBehaviour
    {
        public MapBanner(IntPtr intPtr) : base(intPtr)
        {
        }

        private LIMetadata? _currentMap = null;
        private SpriteRenderer? _thumbnail = null;
        private TMPro.TMP_Text? _title = null;
        private TMPro.TMP_Text? _author = null;
        private TMPro.TMP_Text? _description = null;
        private PassiveButton? _downloadButton = null;
        private PassiveButton? _playButton = null;
        private PassiveButton? _randomButton = null;
        private PassiveButton? _trashButton = null;
        private PassiveButton? _remixButton = null;
        private PassiveButton? _externalButton = null;
        private RandomOverlay? _randomOverlay = null;
        private bool _isInLobby
        {
            get
            {
                return LobbyBehaviour.Instance != null;
            }
        }

        /// <summary>
        /// Sets map metadata for banner to display
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
            GetThumbnail();
        }

        /// <summary>
        /// Updates the interactable state of all buttons
        /// </summary>
        private void UpdateButtons()
        {
            bool isLoaded = _currentMap != null;
            bool isDownloaded = MapFileAPI.Instance?.Exists(_currentMap?.id) ?? false;
            bool isOnline = !string.IsNullOrEmpty(_currentMap?.authorID) && Guid.TryParse(_currentMap.id, out _);
            bool isPublic = _currentMap?.isPublic ?? false;
            bool isRemix = _currentMap?.remixOf != null;

            _playButton?.SetButtonEnableState(isLoaded && isDownloaded && (isOnline || !_isInLobby));
            _randomButton?.SetButtonEnableState(isLoaded && isDownloaded && isOnline);
            _trashButton?.SetButtonEnableState(isLoaded && isDownloaded && isPublic);
            _downloadButton?.SetButtonEnableState(isLoaded && !isDownloaded && isPublic);
            _remixButton?.SetButtonEnableState(isLoaded && isRemix);
            _externalButton?.gameObject.SetActive(isLoaded && isOnline);
        }

        /// <summary>
        /// Event that is called when the download button is pressed
        /// </summary>
        public void OnDownloadClick()
        {
            ShopManager.Instance?.SetOverlayEnabled(true);
            OnDownloadProgress(0);
            LevelImposterAPI.DownloadMap(new Guid(_currentMap?.id ?? ""), OnDownloadProgress, OnDownload, OnError);
        }

        /// <summary>
        /// Event that is called when the <c>LIMap</c> is downloaded
        /// </summary>
        /// <param name="map"></param>
        [HideFromIl2Cpp]
        private void OnDownload(LIMap map)
        {
            MapFileAPI.Instance?.Save(map);
            ShopManager.Instance?.SetOverlayEnabled(false);
            ShopManager.RegenerateFallbackMap();
            UpdateButtons();
        }

        /// <summary>
        /// Callback on download progress
        /// </summary>
        /// <param name="progress">Value from 0 to 1</param>
        private void OnDownloadProgress(float progress)
        {
            int progressPercent = (int)(progress * 100);
            ShopManager.Instance?.SetOverlayText($"<b>Downloading {_currentMap?.name ?? "map"}...</b>\n{progressPercent}%");
        }

        /// <summary>
        /// Event that is called when there is a download error
        /// </summary>
        /// <param name="map"></param>
        [HideFromIl2Cpp]
        private void OnError(string error)
        {
            LILogger.Error(error);
            ShopManager.Instance?.SetOverlayEnabled(false);
            // TODO: Show error popup
        }

        /// <summary>
        /// Event that is called when the play button is pressed
        /// </summary>
        public void OnPlayClick()
        {
            if (_currentMap == null)
                return;
            if (_isInLobby)
                ShopManager.Instance?.SelectMap(_currentMap.id);
            else
                ShopManager.Instance?.LaunchMap(_currentMap.id);
        }

        /// <summary>
        /// Opens the random overlay
        /// </summary>
        public void OnRandomClick()
        {
            if (_currentMap == null)
                return;
            _randomOverlay?.Open(_currentMap.id);
        }

        /// <summary>
        /// Event that is called when the delete button is pressed
        /// </summary>
        public void OnDeleteClick()
        {
            if (_currentMap == null)
                return;
            MapFileAPI.Instance?.Delete(_currentMap?.id ?? "");
            UpdateButtons();
            ShopManager.RegenerateFallbackMap();
        }

        /// <summary>
        /// Event that is called when the external button is pressed
        /// </summary>
        public void OnExternalClick()
        {
            if (_currentMap == null)
                return;
            ShopManager.Instance?.OnExternal(_currentMap.id);
        }

        /// <summary>
        /// Updates the map banner's active thumbnail
        /// </summary>
        private void GetThumbnail()
        {
            if (string.IsNullOrEmpty(_currentMap?.thumbnailURL))
                return;
            if (ThumbnailCacheAPI.Instance?.Exists(_currentMap.id) ?? false)
            {
                ThumbnailCacheAPI.Instance.Get(_currentMap.id, (sprite) =>
                {
                    if (_thumbnail != null)
                        _thumbnail.sprite = sprite;
                });
            }
            else
            {
                LevelImposterAPI.DownloadThumbnail(_currentMap, (sprite) =>
                {
                    if (_thumbnail != null)
                        _thumbnail.sprite = sprite;
                });
            }
        }

        public void Awake()
        {
            _thumbnail = transform.Find("Thumbnail")?.GetComponent<SpriteRenderer>();
            _title = transform.Find("Title")?.GetComponent<TMPro.TMP_Text>();
            _author = transform.Find("Author")?.GetComponent<TMPro.TMP_Text>();
            _description = transform.Find("Description")?.GetComponent<TMPro.TMP_Text>();
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
    }
}