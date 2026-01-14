using System;
using System.Collections.Generic;
using System.Diagnostics;
using LevelImposter.FileIO;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
///     Manages each of the tabs in the shop
/// </summary>
public class ShopTabs(IntPtr ptr) : MonoBehaviour(ptr)
{
    private List<ShopTab>? _tabs = new();

    public void Awake()
    {
        // Tabs
        _tabs?.Add(new ShopTab(transform, "DownloadsTab", ShopManager.Tab.Downloads));
        _tabs?.Add(new ShopTab(transform, "FeaturedTab", ShopManager.Tab.Featured));
        _tabs?.Add(new ShopTab(transform, "TopTab", ShopManager.Tab.Top));
        _tabs?.Add(new ShopTab(transform, "RecentTab", ShopManager.Tab.Recent));

        // Other Buttons
        var exitButton = transform.Find("ExitButton")?.GetComponent<PassiveButton>();
        var folderButton = transform.Find("FolderButton")?.GetComponent<PassiveButton>();
        exitButton?.OnClick.AddListener((Action)ShopManager.CloseShop);
        folderButton?.OnClick.AddListener((Action)OpenFolder);
    }

    public void OnDestroy()
    {
        _tabs = null;
    }

    /// <summary>
    ///     Updates the state of all tab buttons
    /// </summary>
    public void UpdateButtons()
    {
        if (_tabs == null)
            return;
        foreach (var tab in _tabs)
            tab.UpdateButton();
    }

    /// <summary>
    ///     Opens the folder where the maps are stored
    /// </summary>
    public void OpenFolder()
    {
        // TODO: Check if the current platform is supported (windows, linux, etc.)
        Process.Start("explorer.exe", MapFileAPI.GetDirectory());
    }

    /// <summary>
    ///     Represents a single shop tab
    /// </summary>
    public class ShopTab
    {
        private readonly PassiveButton? _button;
        private readonly GameObject? _highlightSprite;
        private readonly ShopManager.Tab _tab;
        private readonly Sprite? _titleSprite;

        public ShopTab(Transform parent, string name, ShopManager.Tab tab)
        {
            _button = parent.Find(name)?.GetComponent<PassiveButton>();
            _highlightSprite = _button?.transform.Find("Highlight")?.gameObject;
            _titleSprite = _button?.transform.Find("Title")?.GetComponent<SpriteRenderer>()?.sprite;
            _tab = tab;

            _button?.OnClick.AddListener((Action)OnClick);
        }

        public void UpdateButton()
        {
            _highlightSprite?.SetActive(ShopManager.Instance?.CurrentTab == _tab);
        }

        public void OnClick()
        {
            ShopManager.Instance?.SetTab(_tab);
            if (ShopManager.Instance?.Title != null)
                ShopManager.Instance.Title.sprite = _titleSprite;
        }
    }
}