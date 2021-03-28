using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Models
{
    class AssetData
    {
        public ShipStatus.MapType map;
        public ObjType objType;

        public string name;
        public string spriteRendererName;
        public string mapObjName;
        public string shipBehaviorName;

        public SpriteRenderer spriteRenderer;
        public GameObject mapObj;
        public MonoBehaviour shipBehavior;
    }

    enum ObjType
    {
        Other,
        CommonTask,
        ShortTask,
        LongTask
    }
}
