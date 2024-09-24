using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class OneWayColliderBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-onewaycollider")
            return;

        // Room Component
        var systemType = RoomBuilder.GetParentOrDefault(elem);
        var shipRoom = RoomBuilder.GetShipRoom(systemType);
        if (shipRoom == null)
        {
            LILogger.Warn($"{elem.name} has no room attatched.");
            return;
        }

        // Iterate through shadow children
        for (var i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            var isShadow = child.gameObject.layer == (int)Layer.Shadow;

            // Add Component to Shadows
            if (isShadow)
            {
                var shadowComponent = child.gameObject.AddComponent<OneWayShadows>();
                shadowComponent.RoomCollider = shipRoom.roomArea;
                shadowComponent.IgnoreImpostor = elem.properties.isImposterIgnored ?? false;
            }
        }
    }
}