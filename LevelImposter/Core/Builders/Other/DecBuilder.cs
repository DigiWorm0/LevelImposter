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

            // Sprite
            var spriteRenderer = MapUtils.CloneSprite(obj, prefab);

            // Fixes Pivot Offset Bug
            if (_fixTypes.Contains(elem.type))
            {
                Sprite sprite = Sprite.Create(
                    spriteRenderer.sprite.texture,
                    spriteRenderer.sprite.rect,
                    new Vector2(0.5f, 0.5f)
                );
                spriteRenderer.sprite = sprite;
                sprite.hideFlags = HideFlags.HideAndDontSave;
            }
            if (isRoom)
                obj.layer = (int)Layer.Ship;
        }

        public void PostBuild() { }
    }
}