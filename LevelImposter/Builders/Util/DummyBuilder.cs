using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class DummyBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-dummy")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Add Location
        shipStatus.DummyLocations = MapUtils.AddToArr(shipStatus.DummyLocations, obj.transform);
    }
}