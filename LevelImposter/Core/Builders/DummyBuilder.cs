using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class DummyBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-dummy")
                return;

            ShipStatus shipStatus = LIShipStatus.Instance.shipStatus;
            shipStatus.DummyLocations = MapUtils.AddToArr(shipStatus.DummyLocations, obj.transform);
        }

        public void PostBuild() { }
    }
}
