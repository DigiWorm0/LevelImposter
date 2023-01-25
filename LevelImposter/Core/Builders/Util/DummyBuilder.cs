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
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");

            // Add Location
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;
            shipStatus.DummyLocations = MapUtils.AddToArr(shipStatus.DummyLocations, obj.transform);

            // TODO: Customize each dummy location with name/outfit
        }

        public void PostBuild() { }
    }
}
