using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Builders.Util;

public class DummyBuilder : IElemBuilder
{
    public static Dictionary<Guid, int> DummyIndex { get; } = new();

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-dummy")
            return;
        if (LIShipStatus.GetInstance().ShipStatus is not ShipStatus ship)
            return;


        // Add location and save its index with the element id (see DummyPatch)
        DummyIndex[elem.id] = ship.DummyLocations.Length;
        ship.DummyLocations = ship.DummyLocations.Add(obj.transform);
    }
}