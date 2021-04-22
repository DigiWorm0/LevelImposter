using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class DecBuilder : Builder
    {
        private PolusHandler polus;

        public DecBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool PreBuild(MapAsset asset)
        {
            if (!asset.type.StartsWith("dec-"))
                return true;
            DecData utilData = AssetDB.dec[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            spriteRenderer.material = utilData.SpriteRenderer.material;
            obj.layer = (int)Layer.ShortObjects;

            // Colliders
            AssetHelper.BuildColliders(asset, obj, utilData.Scale);
            
            polus.Add(obj, asset, utilData.Scale);

            return true;
        }

        public bool PostBuild()
        {
            return true;
        }
    }
}
