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

        public bool PreBuild(MapAsset asset)
        {
            if (asset.type != "custom" && asset.spriteType != "custom")
                return true;

            GameObject obj = new GameObject("Custom Asset");

            SpriteRenderer render = obj.AddComponent<SpriteRenderer>();
            render.sprite = AssetHelper.SpriteFromBase64(asset.type);
            obj.layer = (int)Layer.Ship;

            // Colliders
            AssetHelper.BuildColliders(asset, obj);

            // Polus
            polus.Add(obj, asset);
            return true;
        }

        public bool PostBuild()
        {
            return true;
        }
    }
}
