using LevelImposter.Core;
using System;
using UnityEngine;
using TMPro;

namespace LevelImposter.Shop
{
    public static class MainMenuBuilder
    {
        private static Sprite? _menuIcon = null;

        private const string BUTTON_PATH = "MainMenuManager/MainUI/AspectScaler/LeftPanel/Main Buttons/Inventory Button";
        private const string TEXT_PATH = "FontPlacer/Text_TMP";
        private static readonly string[] ICON_PATHS = new string[]
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
            text.GetComponent<TMP_Text>().text = "Maps";
            GameObject.Destroy(text.GetComponent<TextTranslatorTMP>());

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
                _menuIcon = MapUtils.LoadSpriteResource("MapIcon.png");
            if (_menuIcon == null)
                throw new Exception("The \"MapIcon.png\" resource was not found in assembly");
            return _menuIcon;
        }
    }
}
