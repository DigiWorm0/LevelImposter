using TMPro;
using HarmonyLib;
using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
        public static void Postfix(VersionShower __instance)
        {
            string antiPiracy = Guid.NewGuid().ToString();
            byte[] logoData = Properties.Resources.logo;

            GameObject logoObj = new GameObject("LevelImposterVersion " + antiPiracy);
            logoObj.transform.SetParent(__instance.transform.parent);
            logoObj.transform.localPosition = new Vector3(-4.5f, 1.8f, -1.0f);
            logoObj.layer = (int)Layer.UI;

            SpriteRenderer logoRenderer = logoObj.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, logoData);
            logoRenderer.sprite = Sprite.Create(
                tex,
                new Rect(0.0f, 0.0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                180.0f
            );

            GameObject logoTextObj = new GameObject("LevelImposterText " + antiPiracy);
            logoTextObj.transform.SetParent(logoObj.transform);
            logoTextObj.transform.localPosition = new Vector3(1.78f, 0, 0);

            RectTransform logoTransform = logoTextObj.AddComponent<RectTransform>();
            logoTransform.sizeDelta = new Vector2(2, 0.19f);

            TextMeshPro logoText = logoTextObj.AddComponent<TextMeshPro>();
            logoText.fontSize = 1.5f;
            logoText.alignment = TextAlignmentOptions.BottomLeft;
            logoText.raycastTarget = false;
            logoText.SetText("v" + LevelImposter.VERSION);

            
        }
    }
}
