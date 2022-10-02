using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    public class DecData : AssetData
    {
        public string SpriteRendererName { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }

        public override void ImportMap(GameObject map, ShipStatus shipStatus)
        {
            SpriteRenderer = MapSearchUtil.SearchComponent<SpriteRenderer>(map, SpriteRendererName);
        }
    }
}
