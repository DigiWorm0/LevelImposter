using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class MinimapBuilder : IElemBuilder
{
    public const float DEFAULT_SCALE = 4.975f;

    private bool _isBuilt;

    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-minimap")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstanceOrNull()?.ShipStatus;
        if (shipStatus == null)
            throw new MissingShipException();

        // Check Singleton
        if (_isBuilt)
        {
            LILogger.Warn("Only 1 minimap object should be used per map");
            return;
        }

        // Minimap
        var mapBehaviour = GetMinimap();

        // Map Scale
        var mapScaleVal = elem.properties.minimapScale == null ? 1 : (float)elem.properties.minimapScale;
        var mapScale = mapScaleVal * DEFAULT_SCALE;
        shipStatus.MapScale = mapScale;
        var mapOffset = -(obj.transform.localPosition / mapScale);

        // Sprite Renderer
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            LILogger.Warn("Minimap object has no sprite attached");
            return;
        }

        // Background
        var background = mapBehaviour.ColorControl.gameObject;
        var bgRenderer = background.GetComponent<SpriteRenderer>();
        background.transform.localPosition = background.transform.localPosition;
        background.transform.localScale = obj.transform.localScale / mapScale;
        background.transform.localRotation = obj.transform.localRotation;

        // On Load
        if (SpriteLoader.Instance == null)
        {
            LILogger.Warn("Spite Loader is not instantiated");
            return;
        }

        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != elem.id || bgRenderer == null)
                return;
            bgRenderer.sprite = spriteRenderer.sprite;
            bgRenderer.color = spriteRenderer.color;
            Object.Destroy(obj);
        };

        // Offsets
        var roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);
        roomNames.localPosition = mapOffset;
        var hereIndicatorParent = mapBehaviour.transform.FindChild("HereIndicatorParent");
        hereIndicatorParent.localPosition = mapOffset + new Vector3(0, 0, -0.1f);
        mapBehaviour.countOverlay.transform.localPosition = mapOffset;
        mapBehaviour.infectedOverlay.transform.localPosition = mapOffset;

        _isBuilt = true;
    }

    public void PostBuild()
    {
        if (!_isBuilt)
        {
            var mapBehaviour = GetMinimap();
            mapBehaviour.ColorControl.gameObject.SetActive(false);
            mapBehaviour.transform.FindChild("HereIndicatorParent").gameObject.SetActive(false);
            mapBehaviour.transform.FindChild("RoomNames").gameObject.SetActive(false);
        }

        _isBuilt = false;
    }

    /// <summary>
    ///     Get the current Minimap Behaviour
    /// </summary>
    /// <returns>The current Minimap Behaviour</returns>
    public static MapBehaviour GetMinimap()
    {
        // ShipStatus
        var shipStatus = LIShipStatus.GetInstanceOrNull()?.ShipStatus;
        if (shipStatus == null)
            throw new MissingShipException();

        // Minimap Prefab
        var mapBehaviour = MapBehaviour.Instance;
        if (mapBehaviour == null)
        {
            mapBehaviour = Object.Instantiate(
                shipStatus.MapPrefab,
                DestroyableSingleton<HudManager>.Instance.transform
            );
            mapBehaviour.gameObject.SetActive(false);
        }

        return mapBehaviour;
    }
}