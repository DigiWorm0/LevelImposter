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

        public bool Build(MapAsset asset)
        {
            RoomData utilData = AssetDB.room[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            spriteRenderer.material = utilData.SpriteRenderer.material;
            obj.layer = (int)Layer.Ship;

            // Colliders
            AssetBuilder.BuildColliders(asset, obj);

            polus.Add(obj, asset, utilData.MapType);

            return true;
        }
    }
}
