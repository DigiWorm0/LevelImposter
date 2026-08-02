using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders.Util;

public class DummyBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-dummy")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // Add Location
        shipStatus.DummyLocations = shipStatus.DummyLocations.Add(obj.transform);

        // TODO: Customize each dummy location with name/outfit
    }
}