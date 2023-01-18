using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class DecBuilder : IElemBuilder
    {
        private static readonly List<string> _fixTypes = new()
        {
            "room-dropship"
        };

        public void Build(LIElement elem, GameObject obj)
        {
            bool isDecoration = elem.type.StartsWith("dec-");
            bool isRoom = elem.type.StartsWith("room-");
            if (!(isDecoration || isRoom))
                return;

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

            // Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = prefabRenderer.sprite;

                // Fixes Pivot Offset Bug
                if (_fixTypes.Contains(elem.type))
                {
                    Sprite sprite = Sprite.Create(
                        spriteRenderer.sprite.texture,
                        spriteRenderer.sprite.rect,
                        new Vector2(0.5f, 0.5f)
                    );
                    spriteRenderer.sprite = sprite;
                    SpriteLoader.Instance?.AddSprite(sprite);
                }

                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            obj.layer = (int)(isDecoration ? Layer.ShortObjects : Layer.Ship);
        }

        public void PostBuild() { }
    }
}