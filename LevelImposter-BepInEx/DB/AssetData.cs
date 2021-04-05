using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    abstract class AssetData
    {
        public string Name { get; set; }
        public MapType MapType { get; set; }

        public abstract void ImportMap(GameObject map, ShipStatus shipStatus);
    }
}
