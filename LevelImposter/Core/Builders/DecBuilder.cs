using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class DecBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type.StartsWith("dec-"))
            {
                DecData utilData = AssetDB.dec[elem.type];

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
                RoomData utilData = AssetDB.room[elem.type];

                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (!spriteRenderer)
                {
                    spriteRenderer = obj.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
                    if (elem.properties.color != null)
                        spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
                }
                obj.layer = (int)Layer.Ship;
            }
        }

        public void PostBuild() { }
    }
}