using System;
using HarmonyLib;
using LevelImposter.Core;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

/*
 *      I swear to god if I find that
 *      any of you patch over my logo
 *      I will swallow your entire house whole.
 */
[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionPatch
{
    private static Sprite? _logoSprite;

    public static GameObject? VersionObject { get; private set; }

    public static void Postfix()
    {
        if (!GameState.IsInMainMenu)
            return;

        var antiPiracy = Guid.NewGuid().ToString();

        VersionObject = new GameObject("LevelImposterVersion " + antiPiracy);
        VersionObject.transform.localScale = new Vector3(0.55f, 0.55f, 1.0f);
        VersionObject.layer = (int)Layer.UI;

        var logoPosition = VersionObject.AddComponent<AspectPosition>();
        logoPosition.Alignment = AspectPosition.EdgeAlignments.Right;
        logoPosition.DistanceFromEdge = new Vector3(1.8f, -2.3f, 4.5f);
        logoPosition.AdjustPosition();

        var logoRenderer = VersionObject.AddComponent<SpriteRenderer>();
        logoRenderer.sprite = GetLogoSprite();

        GameObject logoTextObj = new("LevelImposterText " + antiPiracy);
        logoTextObj.transform.SetParent(VersionObject.transform);
        logoTextObj.transform.localPosition = new Vector3(3.2f, 0, 0);

        var logoTransform = logoTextObj.AddComponent<RectTransform>();
        logoTransform.sizeDelta = new Vector2(2, 0.19f);

        var logoText = logoTextObj.AddComponent<TextMeshPro>();
        logoText.fontSize = 1.5f;
        logoText.alignment = TextAlignmentOptions.BottomLeft;
        logoText.raycastTarget = false;
        logoText.SetText("v" + LevelImposter.DisplayVersion);
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