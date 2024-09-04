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

    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-minimapsprite")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstanceOrNull()?.ShipStatus;
        if (shipStatus == null)
            throw new MissingShipException();

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
        GameObject spriteObj = new(elem.name);
        spriteObj.layer = (int)Layer.UI;
        spriteObj.transform.SetParent(parentTransform);
        spriteObj.transform.localPosition = new Vector3(
            elem.x / mapScale,
            elem.y / mapScale,
            elem.z
        );
        spriteObj.transform.localScale = new Vector3(elem.xScale, elem.yScale, 1);
        spriteObj.transform.localRotation = Quaternion.Euler(0, 0, elem.rotation);

        // Sprite Renderer
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            LILogger.Warn("util-minimapsprite does not have a sprite attatched");
            return;
        }

        // Background
        if (SpriteLoader.Instance == null)
        {
            LILogger.Warn("Spite Loader is not instantiated");
            return;
        }

        var bgRenderer = spriteObj.AddComponent<SpriteRenderer>();
        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != elem.id || bgRenderer == null)
                return;
            bgRenderer.sprite = spriteRenderer.sprite;
            bgRenderer.color = spriteRenderer.color;
            Object.Destroy(obj);
        };
    }

    public void PostBuild()
    {
    }
}