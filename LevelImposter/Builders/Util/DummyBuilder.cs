using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class DummyBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-dummy")
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Add Location
            shipStatus.DummyLocations = MapUtils.AddToArr(shipStatus.DummyLocations, obj.transform);

            // TODO: Customize each dummy location with name/outfit
        }

        public void PostBuild() { }
    }
}
