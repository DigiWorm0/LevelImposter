using TMPro;
using HarmonyLib;
using LevelImposter.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelImposter.Shop
{
    /*
     *      I swear to god if I find that
     *      any of you patch over my logo
     *      I will swallow your entire house whole.
     */
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionPatch
    {
        private static Sprite? _logoSprite = null;

        public static void Postfix(VersionShower __instance)
        {
            bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
            if (!isMainMenu)
                return;

            string antiPiracy = Guid.NewGuid().ToString();

            GameObject logoObj = new("LevelImposterVersion " + antiPiracy);
            logoObj.transform.SetParent(__instance.transform.parent);
            logoObj.transform.localScale = new Vector3(0.55f, 0.55f, 1.0f);
            logoObj.transform.position = new Vector3(4f, -2.3f, -1.0f);
            logoObj.layer = (int)Layer.UI;

            SpriteRenderer logoRenderer = logoObj.AddComponent<SpriteRenderer>();
            logoRenderer.sprite = GetLogoSprite();

            GameObject logoTextObj = new("LevelImposterText " + antiPiracy);
            logoTextObj.transform.SetParent(logoObj.transform);
            logoTextObj.transform.localPosition = new Vector3(3.55f, 0, 0);

            RectTransform logoTransform = logoTextObj.AddComponent<RectTransform>();
            logoTransform.sizeDelta = new Vector2(2, 0.19f);

            TextMeshPro logoText = logoTextObj.AddComponent<TextMeshPro>();
            logoText.fontSize = 1.5f;
            logoText.alignment = TextAlignmentOptions.BottomLeft;
            logoText.raycastTarget = false;
            logoText.SetText("v" + LevelImposter.Version);
        }

        private static Sprite GetLogoSprite()
        {
            if (_logoSprite == null)
                _logoSprite = MapUtils.LoadSpriteResource("LevelImposterLogo.png");
            if (_logoSprite == null)
                throw new Exception("The \"LevelImposterLogo.png\" resource was not found in assembly");
            return _logoSprite;
        }
    }
}
