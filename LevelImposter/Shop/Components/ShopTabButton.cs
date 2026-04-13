using System;
using System.Collections.Generic;
using System.Diagnostics;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.FileIO;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
///     Manages each of the tabs in the shop
/// </summary>
public class ShopTabButton(IntPtr ptr) : MonoBehaviour(ptr)
{
    public Il2CppValueField<int> tab;
    public Il2CppReferenceField<Sprite> titleSprite;

    public ShopTab TabType => (ShopTab)tab.Value;
    
    private PassiveButton? _passiveButton;
    
    public void Awake()
    {
        _passiveButton = GetComponent<PassiveButton>();
        _passiveButton.OnClick.AddListener((Action)OnButtonClick);
    }

    /// <summary>
    /// Sets whether this tab is selected or not
    /// </summary>
    /// <param name="isSelected">Whether the tab is selected</param>
    public void SetTabSelected(bool isSelected)
    {
        _passiveButton?.SelectButton(isSelected);
    }

    /// <summary>
    /// Called when the button is clicked
    /// </summary>
    private void OnButtonClick()
    {
        ShopManager.Instance?.SetTab((ShopTab)tab.Value, titleSprite.Value);
    }
}