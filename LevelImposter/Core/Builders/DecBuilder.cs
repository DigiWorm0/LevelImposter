using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class DecBuilder : IElemBuilder
    {
        private static readonly List<string> _pivotShiftTypes = new()
        {
            "room-dropship"
        };

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type.StartsWith("dec-"))
            {
                DecData utilData = AssetDB.Decor[elem.type];

                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (!spriteRenderer)
                {
                    spriteRenderer = obj.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = utilData.SpriteRenderer.sprite;

                    if (elem.properties.color != null)
                        spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
                }
                obj.layer = (int)Layer.ShortObjects;
            }
            else if (elem.type.StartsWith("room-"))
            {
                RoomData utilData = AssetDB.Room[elem.type];

                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (!spriteRenderer)
                {
                    spriteRenderer = obj.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = utilData.SpriteRenderer.sprite;

                    // Fixes Pivot Offset Bug
                    if (_pivotShiftTypes.Contains(elem.type))
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
                obj.layer = (int)Layer.Ship;
            }
        }

        public void PostBuild() { }
    }
}