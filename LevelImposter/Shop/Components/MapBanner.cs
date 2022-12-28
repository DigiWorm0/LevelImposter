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

        private LIMetadata _currentMap;
        private GameObject _loadingOverlay;
        private GameObject _errOverlay;
        private Image _thumbnail;
        private TMPro.TMP_Text _nameText;
        private TMPro.TMP_Text _authorText;
        private TMPro.TMP_Text _descText;
        private Button _downloadButton;
        private Button _playButton;
        private Button _deleteButton;
        private Button _externalButton;
        private bool _isCustomTex = false;
        private bool _isEnabled = true;
        private bool _isInLobby
        {
            get
            {
                return SceneManager.GetActiveScene().name != "HowToPlay";
            }
        }

        public void Awake()
        {
            _loadingOverlay = transform.FindChild("LoadOverlay").gameObject;
            _errOverlay = transform.FindChild("ErrOverlay").gameObject;
            _thumbnail = transform.FindChild("Thumbnail").GetComponent<Image>();
            _nameText = transform.FindChild("Title").GetComponent<TMPro.TMP_Text>();
            _authorText = transform.FindChild("Author").GetComponent<TMPro.TMP_Text>();
            _descText = transform.FindChild("Description").GetComponent<TMPro.TMP_Text>();
            _downloadButton = transform.FindChild("DownloadBtn").GetComponent<Button>();
            _playButton = transform.FindChild("PlayBtn").GetComponent<Button>();
            _deleteButton = transform.FindChild("DeleteBtn").GetComponent<Button>();
            _externalButton = transform.FindChild("ExternalBtn").GetComponent<Button>();
        }

        public void Start()
        {
            _downloadButton.onClick.AddListener((Action)OnDownloadClick);
            _playButton.onClick.AddListener((Action)OnPlayClick);
            _deleteButton.onClick.AddListener((Action)OnDeleteClick);
            _externalButton.onClick.AddListener((Action)OnExternalClick);
            UpdateButtons();
        }

        public void OnDestroy()
        {
            if (_isCustomTex)
                Destroy(_thumbnail.sprite.texture);
        }

        [HideFromIl2Cpp]
        public void SetMap(LIMetadata map)
        {
            _currentMap = map;
            _loadingOverlay.SetActive(false);
            _nameText.text = map.name;
            _authorText.text = map.authorName;
            _descText.text = map.description;
            UpdateButtons();
            GetThumbnail();
        }

        public void UpdateButtons()
        {
            if (_currentMap == null || !_isEnabled)
            {
                _downloadButton.interactable = false;
                _playButton.interactable = false;
                _deleteButton.interactable = false;
                _externalButton.interactable = false;
            }
            else
            {
                bool mapExists = MapFileAPI.Instance.Exists(_currentMap.id);
                bool isOnline = !string.IsNullOrEmpty(_currentMap.authorID);
                _downloadButton.interactable = !mapExists && isOnline;
                _playButton.interactable = mapExists && (isOnline || !_isInLobby);
                _deleteButton.interactable = mapExists && isOnline;
                _externalButton.gameObject.SetActive(isOnline);
                _errOverlay.SetActive(mapExists && !isOnline && _isInLobby);
            }
        }

        public void OnDownloadClick()
        {
            _downloadButton.interactable = false;
            _loadingOverlay.SetActive(true);
            ShopManager.Instance.SetEnabled(false);
            LevelImposterAPI.Instance.DownloadMap(new Guid(_currentMap.id), OnDownload);
        }

        [HideFromIl2Cpp]
        private void OnDownload(LIMap map)
        {
            MapFileAPI.Instance.Save(map);
            _loadingOverlay.SetActive(false);
            ShopManager.Instance.SetEnabled(true);
            UpdateButtons();
        }

        public void OnPlayClick()
        {
            if (_isInLobby)
                ShopManager.Instance.SelectMap(_currentMap.id);
            else
                ShopManager.Instance.LaunchMap(_currentMap.id);
        }

        public void OnDeleteClick()
        {
            MapFileAPI.Instance.Delete(_currentMap.id);
            ThumbnailFileAPI.Instance.Delete(_currentMap.id);
            UpdateButtons();
        }

        public void OnExternalClick()
        {
            Application.OpenURL("https://levelimposter.net/#/map/" + _currentMap.id);
        }

        public void GetThumbnail()
        {
            if (string.IsNullOrEmpty(_currentMap.thumbnailURL))
                return;
            if (ThumbnailFileAPI.Instance.Exists(_currentMap.id))
            {
                ThumbnailFileAPI.Instance.Get(_currentMap.id, (Texture2D texture) =>
                {
                    _thumbnail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    _isCustomTex = true;
                });
            }
            else
            {
                LevelImposterAPI.Instance.DownloadThumbnail(_currentMap, (Texture2D texture) =>
                {
                    byte[] textureData = texture.EncodeToPNG();
                    ThumbnailFileAPI.Instance.Save(_currentMap.id, textureData);
                    _thumbnail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    _isCustomTex = true;
                    textureData = null;
                });
            }
        }

        public void SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
            UpdateButtons();
        }
    }
}