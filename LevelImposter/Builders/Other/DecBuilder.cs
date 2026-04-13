using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

public class DecBuilder : IElemBuilder
{
    private static readonly List<string> TypesToResetPivot = ["room-dropship"];

    public void OnBuild(LIElement elem, GameObject obj)
    {
        var isDecoration = elem.type.StartsWith("dec-");
        var isRoom = elem.type.StartsWith("room-");
        if (!(isDecoration || isRoom))
            return;

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;

        // Sprite
        var spriteRenderer = MapUtils.CloneSprite(obj, prefab);

        // Fixes Pivot Offset Bug
        if (TypesToResetPivot.Contains(elem.type))
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
            obj.layer = (int)Layer.Ship;
    }
}