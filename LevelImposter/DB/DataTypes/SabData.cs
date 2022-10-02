using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    class SabData : AssetData
    {
        public string SpriteRendererName { get; set; }
        public string GameObjName { get; set; }
        public string BehaviorName { get; set; }

        public SpriteRenderer SpriteRenderer { get; set; }
        public GameObject GameObj { get; set; }
        public PlayerTask Behavior { get; set; }

        public override void ImportMap(GameObject map, ShipStatus shipStatus)
        {
            SpriteRenderer = MapSearchUtil.SearchComponent<SpriteRenderer>(map, SpriteRendererName);
            GameObj = MapSearchUtil.SearchChildren(map, GameObjName);
            if (!string.IsNullOrEmpty(BehaviorName))
                Behavior = MapSearchUtil.SearchList(shipStatus.SpecialTasks, BehaviorName);
        }
    }
}
