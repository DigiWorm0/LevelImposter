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
    class RoomBuilder : Builder
    {
        private PolusHandler polus;

        public RoomBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool PreBuild(MapAsset asset)
        {
            if (!asset.type.StartsWith("room-"))
                return true;
            RoomData utilData = AssetDB.room[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            spriteRenderer.material = utilData.SpriteRenderer.material;
            obj.layer = (int)Layer.Ship;

            // Colliders
            AssetHelper.BuildColliders(asset, obj, utilData.Scale);

            // Add to Polus
            Vector3 bounds = spriteRenderer.sprite.bounds.center;
            polus.Add(obj, asset, utilData.Scale, bounds.x, bounds.y);

            return true;
        }

        public bool PostBuild()
        {
            return true;
        }
    }
}
