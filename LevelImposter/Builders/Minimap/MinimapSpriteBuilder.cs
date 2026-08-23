using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Minimap;

internal static class MinimapSpriteBuilder
{
    public static int SabCount { get; private set; }

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        SabCount = 0;
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes = ["util-minimapsprite"]
    )]
    public static void Build(ShipStatus shipStatus, LIElement element, GameObject gameObject)
    {
        // Minimap
        var mapBehaviour = MinimapBuilder.GetMinimap();
        var infectedOverlay = mapBehaviour.infectedOverlay;
        var taskOverlay = mapBehaviour.taskOverlay;
        var imposterOnly = element.properties.imposterOnly == true;
        var parentTransform = imposterOnly ? infectedOverlay.transform : taskOverlay.transform;
        if (imposterOnly)
            SabCount++;

        // GameObject
        var mapScale = shipStatus.MapScale;
        gameObject.layer = (int)Layer.UI;
        gameObject.transform.SetParent(parentTransform, false);
        gameObject.transform.localPosition = new Vector3(
            element.x / mapScale,
            element.y / mapScale,
            element.z
        );
    }
}