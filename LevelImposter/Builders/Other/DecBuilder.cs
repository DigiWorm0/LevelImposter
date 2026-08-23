using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Other;

internal static class DecBuilder
{
    private static readonly List<string> TypesToResetPivot = ["room-dropship"];

    [ElementBuilder]
    public static void Build(LIElement element, GameObject gameObject)
    {
        var isDecoration = element.type.StartsWith("dec-");
        var isRoom = element.type.StartsWith("room-");
        if (!(isDecoration || isRoom))
            return;

        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;

        // Sprite
        var spriteRenderer = gameObject.CloneSprite(prefab);

        // Fixes Pivot Offset Bug
        if (TypesToResetPivot.Contains(element.type))
        {
            var sprite = Sprite.Create(
                spriteRenderer.sprite.texture,
                spriteRenderer.sprite.rect,
                new Vector2(0.5f, 0.5f),
                100,
                0,
                SpriteMeshType.FullRect
            );
            spriteRenderer.sprite = sprite;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            GCHandler.Register(sprite);
        }

        if (isRoom)
            gameObject.layer = (int)Layer.Ship;
    }
}