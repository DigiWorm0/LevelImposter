using LevelImposter.Build.Attributes;
using LevelImposter.Build.Builders.Generic;
using LevelImposter.Core.Models;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class FilterBuilder
{
    private static Sprite? _defaultSquare;

    [ElementBuilder(ElementTypes = ["util-filter"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var sporePrefab = PrefabDB.GetObject("util-spore");
        if (sporePrefab == null)
            return;
        var maskPrefab = sporePrefab.transform.FindChild("SporeScreenMask").gameObject;
        var maskPrefabRenderer = maskPrefab.GetComponent<SpriteRenderer>();

        // Create Sprite
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetDefaultSquare();
            spriteRenderer.color = element.properties.color?.ToUnity() ?? Color.white;
        }

        // Create Mask
        var maskObj = Object.Instantiate(gameObject, gameObject.transform);
        maskObj.name = "Mask";
        maskObj.transform.localScale = Vector3.one;
        maskObj.transform.position = new Vector3(
            gameObject.transform.position.x,
            gameObject.transform.position.y,
            10.0f
        );
        var maskRenderer = maskObj.GetComponent<SpriteRenderer>();
        maskRenderer.material = maskPrefabRenderer.material;

        // Update Mask on Sprite Load
        SpriteBuilder.OnSpriteLoad += (loadedElem, _) =>
        {
            if (loadedElem.id != element.id || maskRenderer == null)
                return;
            maskRenderer.sprite = spriteRenderer.sprite;
            maskRenderer.color = spriteRenderer.color;
        };

        // Set Layer
        gameObject.layer = (int)Layer.Ship;
        maskObj.layer = (int)Layer.Ship;
    }

    private static Sprite GetDefaultSquare()
    {
        if (_defaultSquare != null)
            return _defaultSquare;

        // Create Texture
        var texture = new Texture2D(100, 100, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };

        // Fill Texture
        for (var x = 0; x < 100; x++)
        for (var y = 0; y < 100; y++)
            texture.SetPixel(x, y, Color.white);
        texture.Apply();

        // Generate Sprite
        _defaultSquare = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100.0f,
            0,
            SpriteMeshType.FullRect
        );

        return _defaultSquare;
    }
}