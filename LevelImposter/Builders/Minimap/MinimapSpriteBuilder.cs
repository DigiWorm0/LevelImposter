using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class MinimapSpriteBuilder : IElemBuilder
{
    public MinimapSpriteBuilder()
    {
        SabCount = 0;
    }

    public static int SabCount { get; private set; }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-minimapsprite")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // Minimap
        var mapBehaviour = MinimapBuilder.GetMinimap();
        var infectedOverlay = mapBehaviour.infectedOverlay;
        var taskOverlay = mapBehaviour.taskOverlay;
        var imposterOnly = elem.properties.imposterOnly == true;
        var parentTransform = imposterOnly ? infectedOverlay.transform : taskOverlay.transform;
        if (imposterOnly)
            SabCount++;

        // GameObject
        var mapScale = shipStatus.MapScale;
        obj.layer = (int)Layer.UI;
        obj.transform.SetParent(parentTransform, false);
        obj.transform.localPosition = new Vector3(
            elem.x / mapScale,
            elem.y / mapScale,
            elem.z
        );
    }
}