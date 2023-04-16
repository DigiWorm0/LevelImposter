using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LevelImposter.Core;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    public class MapBanner : MonoBehaviour
    {
        public MapBanner(IntPtr intPtr) : base(intPtr)
        {
        }

        private LIMetadata? _currentMap = null;
        private GameObject? _loadingOverlay = null;
        private GameObject? _errOverlay = null;
        private GameObject? _randomOverlay = null;
        private GameObject? _remixIcon = null;
        private Image? _thumbnail = null;
        private TMPro.TMP_Text? _nameText = null;
        private TMPro.TMP_Text? _authorText = null;
        private TMPro.TMP_Text? _descText = null;
        private TMPro.TMP_Text? _remixText = null;
        private TMPro.TMP_Text? _randomText = null;
        private Button? _downloadButton = null;
        private Button? _playButton = null;
        private Button? _deleteButton = null;
        private Button? _randomButton = null;
        private Button? _externalButton = null;
        private Slider? _randomSlider = null;
        private bool _isEnabled = true;
        private bool _isInLobby
        {
            get
            {
                return SceneManager.GetActiveScene().name != "HowToPlay";
            }
        }

        /// <summary>
        /// Sets map metadata for banner to display
        /// </summary>
        /// <param name="map">Map metadata to display</param>
        [HideFromIl2Cpp]
        public void SetMap(LIMetadata map)
        {
            if (_nameText == null || _authorText == null || _descText == null || _randomSlider == null)
                return;
            _currentMap = map;
            _loadingOverlay?.SetActive(false);
            _nameText.text = map.name;
            _authorText.text = map.authorName;
            _descText.text = map.description;
            _randomSlider.value = (ConfigAPI.Instance?.GetMapWeight(_currentMap.id) ?? 1) * 10;
            UpdateButtons();
            GetThumbnail();
            GetRemix();
        }

        /// <summary>
        /// Updates the interactable state of all buttons
        /// </summary>
        private void UpdateButtons()
        {
            if (_downloadButton == null || _playButton == null || _deleteButton == null || _randomButton == null || _externalButton == null)
                return;
            if (_currentMap == null || !_isEnabled)
            {
                _downloadButton.interactable = false;
                _playButton.interactable = false;
                _deleteButton.interactable = false;
                _randomButton.interactable = false;
                _externalButton.gameObject.SetActive(false);
            }
            else
            {
                bool mapExists = MapFileAPI.Instance?.Exists(_currentMap.id) ?? false;
                bool isOnline = !string.IsNullOrEmpty(_currentMap.authorID) && Guid.TryParse(_currentMap.id, out _);
                bool isPublic = _currentMap.isPublic;
                bool isRemix = _currentMap.remixOf != null;
                _downloadButton.interactable = !mapExists && isOnline;
                _playButton.interactable = mapExists && (isOnline || !_isInLobby);
                _deleteButton.interactable = mapExists && isOnline;
                _randomButton.interactable = mapExists && _isInLobby;
                _externalButton.gameObject.SetActive(isOnline && isPublic);
                _errOverlay?.SetActive(mapExists && !isOnline && _isInLobby);
                _remixIcon?.SetActive(isRemix);
                _remixText?.gameObject.SetActive(isRemix);
            }
        }

        /// <summary>
        /// Event that is called when the download button is pressed
        /// </summary>
        public void OnDownloadClick()
        {
            if (_downloadButton == null || _loadingOverlay == null || _currentMap == null)
                return;
            _downloadButton.interactable = false;
            _loadingOverlay.SetActive(true);
            ShopManager.Instance?.SetEnabled(false);
            LevelImposterAPI.Instance?.DownloadMap(new Guid(_currentMap.id), OnDownload, OnError);
        }

        /// <summary>
        /// Event that is called when the <c>LIMap</c> is downloaded
        /// </summary>
        /// <param name="map"></param>
        [HideFromIl2Cpp]
        private void OnDownload(LIMap map)
        {
            MapFileAPI.Instance?.Save(map);
            _loadingOverlay?.SetActive(false);
            ShopManager.Instance?.SetEnabled(true);
            ShopManager.RegenerateFallback(true);
            UpdateButtons();
        }

        /// <summary>
        /// Event that is called when there is a download error
        /// </summary>
        /// <param name="map"></param>
        [HideFromIl2Cpp]
        private void OnError(string error)
        {
            LILogger.Error(error);
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
        /// Event that is called when the delete button is pressed
        /// </summary>
        public void OnDeleteClick()
        {
            if (_currentMap == null)
                return;
            MapFileAPI.Instance?.Delete(_currentMap?.id ?? "");
            ThumbnailFileAPI.Instance?.Delete(_currentMap?.id ?? "");
            UpdateButtons();
            ShopManager.RegenerateFallback(true);
        }

        /// <summary>
        /// Event that is called when the random button is pressed
        /// </summary>
        public void OnRandomClick()
        {
            if (_currentMap == null || _randomOverlay == null)
                return;
            bool isActive = _randomOverlay.active;
            ShopManager.Instance?.CloseAllPopups();
            _randomOverlay.active = !isActive;
        }

        /// <summary>
        /// Event that is called when the external button is pressed
        /// </summary>
        public void OnExternalClick()
        {
            if (_currentMap == null)
                return;
            ShopManager.Instance?.ViewMap(_currentMap.id);
        }

        /// <summary>
        /// Event that is called whenever the random slider is moved
        /// </summary>
        /// <param name="newValue">New value of the slider from 0 to 10</param>
        public void OnRandomSliderChange(float newValue)
        {
            if (_randomText == null || _currentMap == null)
                return;

            float actualValue = newValue * 0.1f;
            bool hasChanged = actualValue != ConfigAPI.Instance?.GetMapWeight(_currentMap.id);
            if (hasChanged)
                ShopManager.RegenerateFallback(true);

            _randomText.text = $"{Mathf.Round(actualValue * 100.0f)}%";
            ConfigAPI.Instance?.SetMapWeight(_currentMap.id, actualValue);
        }

        /// <summary>
        /// Updates the map banner's active thumbnail
        /// </summary>
        private void GetThumbnail()
        {
            if (string.IsNullOrEmpty(_currentMap?.thumbnailURL))
                return;
            if (ThumbnailFileAPI.Instance == null || LevelImposterAPI.Instance == null)
                return;
            if (ThumbnailFileAPI.Instance.Exists(_currentMap.id))
            {
                ThumbnailFileAPI.Instance.Get(_currentMap.id, (sprite) =>
                {
                    if (_thumbnail != null)
                        _thumbnail.sprite = sprite;
                });
            }
            else
            {
                LevelImposterAPI.Instance.DownloadThumbnail(_currentMap, (sprite) =>
                {
                    if (_thumbnail != null)
                        _thumbnail.sprite = sprite;
                });
            }
        }

        /// <summary>
        /// Updates the map banner's remix info
        /// </summary>
        [HideFromIl2Cpp]
        private void GetRemix()
        {
            if (_currentMap?.remixOf == null)
                return;

            if (_remixText != null)
                _remixText.text = "Remix of\n<i>Unknown Map</i>";

            LevelImposterAPI.Instance?.GetMap((Guid)_currentMap.remixOf, OnRemix, OnError);
        }

        /// <summary>
        /// Callback function on remix info
        /// </summary>
        /// <param name="metadata">Remix map metadata</param>
        [HideFromIl2Cpp]
        private void OnRemix(LIMetadata metadata)
        {
            if (_remixText != null)
                _remixText.text = $"Remix of\n<b>{metadata.name}</b> by {metadata.authorName}";
        }

        /// <summary>
        /// Sets the button to be enabled or disabled
        /// </summary>
        /// <param name="isEnabled">TRUE if enabled</param>
        public void SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
            UpdateButtons();
        }

        /// <summary>
        /// Closes all popups open
        /// </summary>
        public void CloseAllPopups()
        {
            if (_randomOverlay == null)
                return;
            _randomOverlay.active = false;
        }

        public void Awake()
        {
            _loadingOverlay = transform.FindChild("LoadOverlay")?.gameObject;
            _errOverlay = transform.FindChild("ErrOverlay")?.gameObject;
            _randomOverlay = transform.FindChild("RandomOverlay")?.gameObject;
            _remixIcon = transform.FindChild("RemixIcon")?.gameObject;
            _thumbnail = transform.FindChild("Thumbnail")?.GetComponent<Image>();
            _nameText = transform.FindChild("Title")?.GetComponent<TMPro.TMP_Text>();
            _authorText = transform.FindChild("Author")?.GetComponent<TMPro.TMP_Text>();
            _descText = transform.FindChild("Description")?.GetComponent<TMPro.TMP_Text>();
            _remixText = transform.FindChild("Remix")?.GetComponent<TMPro.TMP_Text>();
            _randomText = _randomOverlay?.transform.FindChild("Percent")?.GetComponent<TMPro.TMP_Text>();
            _downloadButton = transform.FindChild("DownloadBtn")?.GetComponent<Button>();
            _playButton = transform.FindChild("PlayBtn")?.GetComponent<Button>();
            _randomButton = transform.FindChild("RandomBtn")?.GetComponent<Button>();
            _deleteButton = transform.FindChild("DeleteBtn")?.GetComponent<Button>();
            _externalButton = transform.FindChild("ExternalBtn")?.GetComponent<Button>();
            _randomSlider = _randomOverlay?.transform.FindChild("Slider")?.GetComponent<Slider>();

            if (_loadingOverlay == null)
                LILogger.Warn("Could not find Loading Overlay in Map Banner");
            if (_errOverlay == null)
                LILogger.Warn("Could not find Error Overlay in Map Banner");
            if (_randomOverlay == null)
                LILogger.Warn("Could not find Random Overlay in Map Banner");
            if (_remixIcon == null)
                LILogger.Warn("Could not find Remix Icon in Map Banner");
            if (_thumbnail == null)
                LILogger.Warn("Could not find Thumbnail in Map Banner");
            if (_nameText == null)
                LILogger.Warn("Could not find Name Text in Map Banner");
            if (_authorText == null)
                LILogger.Warn("Could not find Author Text in Map Banner");
            if (_descText == null)
                LILogger.Warn("Could not find Description Text in Map Banner");
            if (_remixText == null)
                LILogger.Warn("Could not find Remix Text in Map Banner");
            if (_randomText == null)
                LILogger.Warn("Could not find Random Text in Map Banner");
            if (_downloadButton == null)
                LILogger.Warn("Could not find Download Button in Map Banner");
            if (_playButton == null)
                LILogger.Warn("Could not find Play Button in Map Banner");
            if (_deleteButton == null)
                LILogger.Warn("Could not find Delete Button in Map Banner");
            if (_randomButton == null)
                LILogger.Warn("Could not find Random Button in Map Banner");
            if (_externalButton == null)
                LILogger.Warn("Could not find External Button in Map Banner");
            if (_randomSlider == null)
                LILogger.Warn("Could not find Random Slider in Map Banner");
        }
        public void Start()
        {
            _downloadButton?.onClick.AddListener((Action)OnDownloadClick);
            _playButton?.onClick.AddListener((Action)OnPlayClick);
            _deleteButton?.onClick.AddListener((Action)OnDeleteClick);
            _randomButton?.onClick.AddListener((Action)OnRandomClick);
            _externalButton?.onClick.AddListener((Action)OnExternalClick);
            UpdateButtons();

            _randomSlider?.onValueChanged.AddListener((Action<float>)OnRandomSliderChange);
        }
        public void OnDestroy()
        {
            _thumbnail = null;
            _currentMap = null;
            _loadingOverlay = null;
            _errOverlay = null;
            _randomOverlay = null;
            _thumbnail = null;
            _nameText = null;
            _authorText = null;
            _descText = null;
            _downloadButton = null;
            _playButton = null;
            _deleteButton = null;
            _randomButton = null;
            _externalButton = null;
            _remixIcon = null;
            _remixText = null;
            _randomText = null;
            _randomSlider = null;
        }
    }
}