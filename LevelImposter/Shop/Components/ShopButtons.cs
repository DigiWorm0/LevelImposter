using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace LevelImposter.Shop
{
    public class ShopButtons : MonoBehaviour
    {
        public ShopButtons(IntPtr intPtr) : base(intPtr)
        {
        }

        public Button DownloadedButton;
        public Button TopButton;
        public Button RecentButton;
        public Button FeaturedButton;
        public Button FolderButton;

        private string _selectedID = "downloaded";
        private bool isEnabled = true;

        public void Start()
        {
            DownloadedButton.onClick.AddListener((Action)OnDownloaded);
            TopButton.onClick.AddListener((Action)OnTop);
            RecentButton.onClick.AddListener((Action)OnRecent);
            FeaturedButton.onClick.AddListener((Action)OnFeatured);
            FolderButton.onClick.AddListener((Action)OnFolder);
            UpdateButtons();
        }

        public void UpdateButtons()
        {
            DownloadedButton.interactable = _selectedID != "downloaded" && isEnabled;
            TopButton.interactable = _selectedID != "top" && isEnabled;
            RecentButton.interactable = _selectedID != "recent" && isEnabled;
            FeaturedButton.interactable = _selectedID != "featured" && isEnabled;
        }

        public void OnDownloaded()
        {
            if (_selectedID != "downloaded")
            {
                _selectedID = "downloaded";
                ShopManager.Instance.ListDownloaded();
                UpdateButtons();
            }
        }

        public void OnTop()
        {
            if (_selectedID != "top")
            {
                _selectedID = "top";
                ShopManager.Instance.ListTop();
                UpdateButtons();
            }
        }

        public void OnRecent()
        {
            if (_selectedID != "recent")
            {
                _selectedID = "recent";
                ShopManager.Instance.ListRecent();
                UpdateButtons();
            }
        }

        public void OnFeatured()
        {
            if (_selectedID != "featured")
            {
                _selectedID = "featured";
                ShopManager.Instance.ListFeatured();
                UpdateButtons();
            }
        }

        public void OnFolder()
        {
            Process.Start("explorer.exe", MapFileAPI.Instance.GetDirectory());
        }

        public void SetEnabled(bool isEnabled)
        {
            this.isEnabled = isEnabled;
            UpdateButtons();
        }
    }
}