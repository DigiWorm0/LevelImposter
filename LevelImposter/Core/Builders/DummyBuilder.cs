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
            LILogger.Info(elem.type);
            if (elem.type != "util-dummy")
                return;
            
            ShipStatus.Instance.DummyLocations = MapUtils.AddToArr(ShipStatus.Instance.DummyLocations, obj.transform);
            LILogger.Info(ShipStatus.Instance.DummyLocations.Length);
        }

        public void PostBuild() { }
    }
}
