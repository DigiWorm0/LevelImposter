using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    class UtilData : AssetData
    {
        public string SpriteRendererName { get; set; }
        public string GameObjName { get; set; }

        public SpriteRenderer SpriteRenderer { get; set; }
        public GameObject GameObj { get; set; }

        public override void ImportMap(GameObject map, ShipStatus shipStatus)
        {
            SpriteRenderer = MapSearcher.SearchComponent<SpriteRenderer>(map, SpriteRendererName);
            GameObj = MapSearcher.SearchChildren(map, GameObjName);
        }
    }
}
