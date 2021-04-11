using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    class SSData : AssetData
    {
        public ShipStatus ShipStatus { get; set; }

        public override void ImportMap(GameObject map, ShipStatus shipStatus)
        {
            ShipStatus = shipStatus;
        }
    }
}
