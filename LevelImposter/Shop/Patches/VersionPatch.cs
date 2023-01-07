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
        public static Sprite? _logoSprite = null;

        public static void Postfix(VersionShower __instance)
        {
            bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
            if (!isMainMenu)
                return;

            string antiPiracy = Guid.NewGuid().ToString();
            byte[] logoData = Properties.Resources.logo;

            GameObject logoObj = new("LevelImposterVersion " + antiPiracy);
            logoObj.transform.SetParent(__instance.transform.parent);
            logoObj.transform.localPosition = new Vector3(4.0f, -2.75f, -1.0f);
            logoObj.layer = (int)Layer.UI;

            SpriteRenderer logoRenderer = logoObj.AddComponent<SpriteRenderer>();
            if (_logoSprite == null)
            {
                Texture2D logoTex = new Texture2D(1, 1);
                ImageConversion.LoadImage(logoTex, logoData);
                _logoSprite = Sprite.Create(
                    logoTex,
                    new Rect(0.0f, 0.0f, logoTex.width, logoTex.height),
                    new Vector2(0.5f, 0.5f),
                    180.0f,
                    0,
                    SpriteMeshType.FullRect
                );
            }
            logoRenderer.sprite = _logoSprite;

            GameObject logoTextObj = new("LevelImposterText " + antiPiracy);
            logoTextObj.transform.SetParent(logoObj.transform);
            logoTextObj.transform.localPosition = new Vector3(1.78f, 0, 0);

            RectTransform logoTransform = logoTextObj.AddComponent<RectTransform>();
            logoTransform.sizeDelta = new Vector2(2, 0.19f);

            TextMeshPro logoText = logoTextObj.AddComponent<TextMeshPro>();
            logoText.fontSize = 1.5f;
            logoText.alignment = TextAlignmentOptions.BottomLeft;
            logoText.raycastTarget = false;
            logoText.SetText("v" + LevelImposter.Version);
        }
    }
    /*
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetStringWithDefault), new System.Type[] { typeof(StringNames), typeof(string) })]
    public static class RegionPatch
    {
        public static void Postfix(ref string __result)
        {
            __result = __result.Replace("LevelImposter", "<color=#176be6>Level</color><color=#c13030>Impostor</color>");
        }
    }
    */
}
