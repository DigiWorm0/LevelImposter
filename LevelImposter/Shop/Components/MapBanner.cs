using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace LevelImposter.Shop
{
    public class MapBanner : MonoBehaviour
    {
        public MapBanner(IntPtr intPtr) : base(intPtr)
        {
        }

        private LIMetadata _currentMap;
        private GameObject _loadingSpinner;
        private Image _thumbnail;
        private TMPro.TMP_Text _nameText;
        private TMPro.TMP_Text _authorText;
        private TMPro.TMP_Text _descText;
        private Button _downloadButton;
        private Button _playButton;
        private Button _deleteButton;
        private Button _externalButton;
        private bool _isCustomTex = false;
        private bool _isInLobby
        {
            get
            {
                return SceneManager.GetActiveScene().name != "HowToPlay";
            }
        }

        public void Awake()
        {
            _loadingSpinner = transform.FindChild("LoadOverlay").gameObject;
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
            _downloadButton.onClick.AddListener((Action)OnDownload);
            _playButton.onClick.AddListener((Action)OnPlay);
            _deleteButton.onClick.AddListener((Action)OnDelete);
            _externalButton.onClick.AddListener((Action)OnExternal);
            UpdateButtons();
        }

        public void OnDestroy()
        {
            if (_isCustomTex)
                Destroy(_thumbnail.sprite.texture);
        }

        public void SetMap(LIMetadata map)
        {
            this._currentMap = map;
            _loadingSpinner.SetActive(false);
            _nameText.text = map.name;
            _authorText.text = map.authorName;
            _descText.text = map.description;
            UpdateButtons();
            GetThumbnail();
        }

        public void UpdateButtons()
        {
            if (_currentMap == null)
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
            }
        }

        public void OnDownload()
        {
            _downloadButton.interactable = false;
            _loadingSpinner.SetActive(true);
            LevelImposterAPI.Instance.DownloadMap(new System.Guid(_currentMap.id), (LIMap map) =>
            {
                MapFileAPI.Instance.Save(map);
                _loadingSpinner.SetActive(false);
                UpdateButtons();
            });
        }

        public void OnPlay()
        {
            if (_isInLobby)
                ShopManager.Instance.SelectMap(_currentMap.id);
            else
                ShopManager.Instance.LaunchMap(_currentMap.id);
        }

        public void OnDelete()
        {
            MapFileAPI.Instance.Delete(_currentMap.id);
            ThumbnailFileAPI.Instance.Delete(_currentMap.id);
            UpdateButtons();
        }

        public void OnExternal()
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
    }
}