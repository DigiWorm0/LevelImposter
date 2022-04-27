using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class DummyBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-dummy")
                return;

            ShipStatus.Instance.DummyLocations = LIShipStatus.AddToArr(ShipStatus.Instance.DummyLocations, obj.transform);
        }

        public void PostBuild() { }
    }
}
