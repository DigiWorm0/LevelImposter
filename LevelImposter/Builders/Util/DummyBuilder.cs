using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class DummyBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-dummy")
            return;

        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Add location and save its index with the element id (see DummyPatch)
        LIShipStatus.GetInstance().DummyIndex[elem.id] = shipStatus.DummyLocations.Length;
        shipStatus.DummyLocations = MapUtils.AddToArr(shipStatus.DummyLocations, obj.transform);
    }
}