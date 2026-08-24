using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class OneWayColliderBuilder
{
    [ElementBuilder(ElementTypes = ["util-onewaycollider"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Room Component
        var systemType = RoomBuilder.GetParentOrDefault(element);
        var shipRoom = RoomBuilder.GetShipRoom(systemType);
        if (shipRoom == null)
        {
            LILogger.Warn($"{element.name} has no room attached.");
            return;
        }

        // Iterate through shadow children
        for (var i = 0; i < gameObject.transform.childCount; i++)
        {
            var child = gameObject.transform.GetChild(i);
            var isShadow = child.gameObject.layer == (int)Layer.Shadow;

            // Add Component to Shadows
            if (!isShadow)
                continue;

            var shadowComponent = child.gameObject.AddComponent<OneWayShadows>();
            shadowComponent.RoomCollider = shipRoom.roomArea;
            shadowComponent.IgnoreImpostor = element.properties.isImposterIgnored ?? false;
        }
    }
}