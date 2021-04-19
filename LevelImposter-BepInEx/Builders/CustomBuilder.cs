using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    class CustomBuilder : Builder
    {
        private PolusHandler polus;

        public CustomBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool Build(MapAsset asset)
        {
            GameObject obj = new GameObject("Custom Asset");

            SpriteRenderer render = obj.AddComponent<SpriteRenderer>();
            render.sprite = AssetBuilder.SpriteFromBase64(asset.type);
            obj.layer = (int)Layer.Ship;

            // Colliders
            AssetBuilder.BuildColliders(asset, obj);

            // Polus
            polus.Add(obj, asset);
            return true;
        }
    }
}
