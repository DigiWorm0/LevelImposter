using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class ShopButtons : MonoBehaviour
    {
        public ShopButtons(IntPtr intPtr) : base(intPtr)
        {
        }

        private Button? _downloadedButton = null;
        private Button? _topButton = null;
        private Button? _recentButton = null;
        private Button? _featuredButton = null;
        private Button? _folderButton = null;
        private string _selectedID = "downloaded";
        private bool isEnabled = true;

        /// <summary>
        /// Updates the interactable state of all buttons
        /// </summary>
        public void UpdateButtons()
        {
            if (_downloadedButton == null || _topButton == null || _recentButton == null || _featuredButton == null)
                return;
            _downloadedButton.interactable = _selectedID != "downloaded" && isEnabled;
            _topButton.interactable = _selectedID != "top" && isEnabled;
            _recentButton.interactable = _selectedID != "recent" && isEnabled;
            _featuredButton.interactable = _selectedID != "featured" && isEnabled;
        }

        /// <summary>
        /// Event that is called when the Downloaded button is clicked
        /// </summary>
        public void OnDownloadedClick()
        {
            if (_selectedID != "downloaded")
            {
                _selectedID = "downloaded";
                ShopManager.Instance.ListDownloaded();
                UpdateButtons();
            }
        }

        /// <summary>
        /// Event that is called when the Top button is clicked
        /// </summary>
        public void OnTopClick()
        {
            if (_selectedID != "top")
            {
                _selectedID = "top";
                ShopManager.Instance.ListTop();
                UpdateButtons();
            }
        }

        /// <summary>
        /// Event that is called when the Recent button is clicked
        /// </summary>
        public void OnRecentClick()
        {
            if (_selectedID != "recent")
            {
                _selectedID = "recent";
                ShopManager.Instance.ListRecent();
                UpdateButtons();
            }
        }

        /// <summary>
        /// Event that is called when the Featured button is clicked
        /// </summary>
        public void OnFeaturedClick()
        {
            if (_selectedID != "featured")
            {
                _selectedID = "featured";
                ShopManager.Instance.ListFeatured();
                UpdateButtons();
            }
        }

        /// <summary>
        /// Event that is called when the Open Folder button is clicked
        /// </summary>
        public void OnOpenFolderClick()
        {
            Process.Start("explorer.exe", MapFileAPI.Instance?.GetDirectory() ?? "/");
        }

        /// <summary>
        /// Enables or disabled the shop buttons
        /// </summary>
        /// <param name="isEnabled">TRUE if the shop buttons should be enabled</param>
        public void SetEnabled(bool isEnabled)
        {
            this.isEnabled = isEnabled;
            UpdateButtons();
        }

        public void Awake()
        {
            Transform btnsParent = transform.FindChild("Canvas").FindChild("Shop Buttons");
            _downloadedButton = btnsParent.FindChild("DownloadedBtn").GetComponent<Button>();
            _topButton = btnsParent.FindChild("TopBtn").GetComponent<Button>();
            _recentButton = btnsParent.FindChild("RecentBtn").GetComponent<Button>();
            _featuredButton = btnsParent.FindChild("FeaturedBtn").GetComponent<Button>();
            _folderButton = btnsParent.FindChild("FolderBtn").GetComponent<Button>();

            if (_downloadedButton == null)
                LILogger.Warn("Could not find Download Button in Shop Buttons");
            if (_topButton == null)
                LILogger.Warn("Could not find Top Button in Shop Buttons");
            if (_recentButton == null)
                LILogger.Warn("Could not find Recent Button in Shop Buttons");
            if (_featuredButton == null)
                LILogger.Warn("Could not find Featured Button in Shop Buttons");
            if (_folderButton == null)
                LILogger.Warn("Could not find Folder Button in Shop Buttons");
        }
        public void Start()
        {
            _downloadedButton?.onClick.AddListener((Action)OnDownloadedClick);
            _topButton?.onClick.AddListener((Action)OnTopClick);
            _recentButton?.onClick.AddListener((Action)OnRecentClick);
            _featuredButton?.onClick.AddListener((Action)OnFeaturedClick);
            _folderButton?.onClick.AddListener((Action)OnOpenFolderClick);
            UpdateButtons();
        }
        public void OnDestroy()
        {
            _downloadedButton = null;
            _topButton = null;
            _recentButton = null;
            _featuredButton = null;
            _folderButton = null;
        }
    }
}