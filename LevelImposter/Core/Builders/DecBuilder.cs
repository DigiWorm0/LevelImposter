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
            if (!elem.type.StartsWith("dec-"))
                return;
            DecData utilData = AssetDB.dec[elem.type];

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
                spriteRenderer.material = utilData.SpriteRenderer.material;
            }
            obj.layer = (int)Layer.ShortObjects;
        }

        public void PostBuild() { }
    }
}
