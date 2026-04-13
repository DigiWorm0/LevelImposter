using System;
using LevelImposter.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders;

public class MinimapBuilder : IElemBuilder
{
    private const float DEFAULT_SCALE = 4.975f;
    
    public int Priority => IElemBuilder.HIGH_PRIORITY; // <-- Ensure other builders can modify the minimap after this

    private bool _isBuilt;

    public void OnPreBuild()
    {
        _isBuilt = false;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-minimap")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // Check Singleton
        if (_isBuilt)
            throw new Exception("Only 1 minimap object should be used per map");

        // Minimap
        var mapBehaviour = GetMinimap();

        // Map Scale
        var mapScaleVal = elem.properties.minimapScale ?? 1;
        shipStatus.MapScale = mapScaleVal * DEFAULT_SCALE;
        var mapOffset = -(obj.transform.localPosition / shipStatus.MapScale);

        // Background
        var background = mapBehaviour.ColorControl.gameObject;
        var bgRenderer = background.GetComponent<SpriteRenderer>();
        background.transform.localPosition = background.transform.localPosition;
        background.transform.localScale = obj.transform.localScale / shipStatus.MapScale;
        background.transform.localRotation = obj.transform.localRotation;

        // Load Sprite
        SpriteBuilder.OnSpriteLoad += (loadedElem, sprite) =>
        {
            if (loadedElem.id != elem.id || bgRenderer == null)
                return;
            bgRenderer.sprite = sprite;
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

    public void OnPostBuild()
    {
        if (_isBuilt)
            return;
        
        // Apply a "default" minimap setup
        var mapBehaviour = GetMinimap();
        mapBehaviour.ColorControl.gameObject.SetActive(false);
        mapBehaviour.transform.FindChild("HereIndicatorParent").gameObject.SetActive(false);
        mapBehaviour.transform.FindChild("RoomNames").gameObject.SetActive(false);
    }

    /// <summary>
    ///     Get the current Minimap Behaviour
    /// </summary>
    /// <returns>The current Minimap Behaviour</returns>
    public static MapBehaviour GetMinimap()
    {
        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;
        if (shipStatus == null)
            throw new MissingShipException();

        // Minimap Prefab
        var mapBehaviour = MapBehaviour.Instance;
        if (mapBehaviour != null)
            return mapBehaviour;
        
        mapBehaviour = Object.Instantiate(
            shipStatus.MapPrefab,
            DestroyableSingleton<HudManager>.Instance.transform
        );
        mapBehaviour.gameObject.SetActive(false);
        return mapBehaviour;
    }
}