using TMPro;
using HarmonyLib;
using LevelImposter.Core;
using System;
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
        private static GameObject? _versionObject = null;

        public static GameObject? VersionObject => _versionObject;

        public static void Postfix()
        {
            bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
            if (!isMainMenu)
                return;

            string antiPiracy = Guid.NewGuid().ToString();

            _versionObject = new("LevelImposterVersion " + antiPiracy);
            _versionObject.transform.localScale = new Vector3(0.55f, 0.55f, 1.0f);
            _versionObject.layer = (int)Layer.UI;

            AspectPosition logoPosition = _versionObject.AddComponent<AspectPosition>();
            logoPosition.Alignment = AspectPosition.EdgeAlignments.Right;
            logoPosition.DistanceFromEdge = new Vector3(1.4f, -2.3f, 0);
            logoPosition.AdjustPosition();

            SpriteRenderer logoRenderer = _versionObject.AddComponent<SpriteRenderer>();
            logoRenderer.sprite = GetLogoSprite();

            GameObject logoTextObj = new("LevelImposterText " + antiPiracy);
            logoTextObj.transform.SetParent(_versionObject.transform);
            logoTextObj.transform.localPosition = new Vector3(3.2f, 0, 0);

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
