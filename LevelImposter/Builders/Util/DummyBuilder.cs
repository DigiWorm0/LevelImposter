using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders.Util;

public static class DummyBuilder
{
    [ElementBuilder(ElementTypes = ["util-dummy"], Target = MapTarget.Game)]
    public static void OnBuild(ShipStatus shipStatus, GameObject gameObject)
    {
        // Add Location
        shipStatus.DummyLocations = shipStatus.DummyLocations.Add(gameObject.transform);

        // TODO: Customize each dummy location with name/outfit
    }
}