using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    class RoomData : AssetData
    {
        public string SpriteRendererName { get; set; }
        public string SpriteName { get; set; }
        public float Scale { get; set; }

        public SpriteRenderer SpriteRenderer { get; set; }

        public override void ImportMap(GameObject map, ShipStatus shipStatus)
        {
            SpriteRenderer = MapSearcher.SearchSprites(map, SpriteRendererName, SpriteName);
        }
    }
}
