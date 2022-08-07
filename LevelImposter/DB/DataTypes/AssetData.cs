using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.DB
{
    public abstract class AssetData
    {
        public string Name { get; set; }
        public MapType MapType { get; set; }

        public abstract void ImportMap(GameObject map, ShipStatus shipStatus);
    }
}
