using LevelImposter.Build.Attributes;
using LevelImposter.Build.Builders.Generic;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class StarfieldBuilder
{
    [ElementBuilder(ElementTypes = ["util-starfield"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var prefab = PrefabDB.GetObject("dec-rock4");
        if (prefab == null)
            return;
        var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

        // Sprite
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            LILogger.Warn($"{element.name} missing a sprite");
        else
            spriteRenderer.material = prefabRenderer.material;

        // Star Prefab
        var starPrefab = Object.Instantiate(gameObject);
        starPrefab.transform.localScale = Vector3.one;
        starPrefab.transform.localRotation = Quaternion.identity;
        var prefabComp = starPrefab.AddComponent<LIStar>();

        var count = element.properties.starfieldCount ?? 20;
        var liStars = new LIStar[count];
        for (var i = 0; i < count; i++)
        {
            var liStar = Object.Instantiate(prefabComp, gameObject.transform);
            liStar.Init(element);
            liStars[i] = liStar;
        }

        Object.Destroy(starPrefab);

        // Load Sprite
        SpriteBuilder.OnSpriteLoad += (loadedElem, _) =>
        {
            if (loadedElem.id != element.id)
                return;

            foreach (var liStar in liStars)
            {
                var starRenderer = liStar.GetComponent<SpriteRenderer>();
                starRenderer.sprite = spriteRenderer?.sprite;
                starRenderer.color = spriteRenderer?.color ?? starRenderer.color;
            }
        };

        // Disable SpriteRenderers
        SpriteRenderer[] spriteRenderers = gameObject.GetComponents<SpriteRenderer>();
        foreach (var renderer in spriteRenderers)
            renderer.enabled = false;

        // Disable Colliders
        Collider2D[] colliders = gameObject.GetComponents<Collider2D>();
        foreach (var collider in colliders)
            collider.enabled = false;
    }
}