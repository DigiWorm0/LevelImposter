using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LevelImposter.Shop
{
    public class ShopButtons : MonoBehaviour
    {
        public Button downloadedButton;
        public Button topButton;
        public Button recentButton;
        public Button featuredButton;

        private string selected = "downloaded";

        public ShopButtons(IntPtr intPtr) : base(intPtr)
        {
        }

        private void Start()
        {
            downloadedButton.onClick.AddListener((Action)OnDownloaded);
            topButton.onClick.AddListener((Action)OnTop);
            recentButton.onClick.AddListener((Action)OnRecent);
            featuredButton.onClick.AddListener((Action)OnFeatured);
            UpdateButtons();
        }

        public void UpdateButtons()
        {
            downloadedButton.interactable = selected != "downloaded";
            topButton.interactable = selected != "top";
            recentButton.interactable = selected != "recent";
            featuredButton.interactable = selected != "featured";
        }

        public void OnDownloaded()
        {
            if (selected != "downloaded")
            {
                selected = "downloaded";
                ShopManager.Instance.ListDownloaded();
                UpdateButtons();
            }
        }

        public void OnTop()
        {
            if (selected != "top")
            {
                selected = "top";
                ShopManager.Instance.ListTop();
                UpdateButtons();
            }
        }

        public void OnRecent()
        {
            if (selected != "recent")
            {
                selected = "recent";
                ShopManager.Instance.ListRecent();
                UpdateButtons();
            }
        }

        public void OnFeatured()
        {
            if (selected != "featured")
            {
                selected = "featured";
                ShopManager.Instance.ListFeatured();
                UpdateButtons();
            }
        }
    }
}