using System;
using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Shop.Builders;

public static class MainMenuBuilder
{
    private const string BUTTON_PATH =
        "MainMenuManager/MainUI/AspectScaler/LeftPanel/Main Buttons/Inventory Button";

    private const string TEXT_PATH = "FontPlacer/Text_TMP";
    private static Sprite? _menuIcon;

    private static readonly string[] ICON_PATHS =
    {
        "Highlight/Icon",
        "Inactive/Icon"
    };

    public static void Build()
    {
        // Button
        var button = GameObject.Find(BUTTON_PATH);

        // Text
        var text = button.transform.Find(TEXT_PATH);
        LITextTranslatorTMP.AddTranslator(text.gameObject, "main_menu.maps");

        // Sprites
        foreach (var path in ICON_PATHS)
        {
            var icon = button.transform.Find(path);
            icon.GetComponent<SpriteRenderer>().sprite = GetIconSprite();
        }
    }

    private static Sprite GetIconSprite()
    {
        if (_menuIcon == null)
            _menuIcon = PackagedResources.LoadSprite("MapIcon.png");
        if (_menuIcon == null)
            throw new Exception("The \"MapIcon.png\" resource was not found in assembly");
        return _menuIcon;
    }
}