using System;
using LevelImposter.Build.Attributes;
using LevelImposter.Build.Builders.Generic;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Build.Builders.Minimap;

internal static class MinimapBuilder
{
    private const float DEFAULT_SCALE = 4.975f;

    private static bool _isBuilt;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        _isBuilt = false;
    }

    [ElementBuilder(
        Priority = Priority.FIRST,
        Target = MapTarget.Game,
        ElementTypes = ["util-minimap"]
    )]
    public static void Build(ShipStatus shipStatus, LIElement element, GameObject gameObject)
    {
        // Check Singleton
        if (_isBuilt)
            throw new Exception("Only 1 minimap object should be used per map");

        // Minimap
        var mapBehaviour = GetMinimap();

        // Map Scale
        var mapScaleVal = element.properties.minimapScale ?? 1;
        shipStatus.MapScale = mapScaleVal * DEFAULT_SCALE;
        var mapOffset = -(gameObject.transform.localPosition / shipStatus.MapScale);

        // Background
        var background = mapBehaviour.ColorControl.gameObject;
        var bgRenderer = background.GetComponent<SpriteRenderer>();
        background.transform.localPosition = background.transform.localPosition;
        background.transform.localScale = gameObject.transform.localScale / shipStatus.MapScale;
        background.transform.localRotation = gameObject.transform.localRotation;

        // Load Sprite
        SpriteBuilder.OnSpriteLoad += (loadedElem, sprite) =>
        {
            if (loadedElem.id != element.id || bgRenderer == null)
                return;
            bgRenderer.sprite = sprite;
            Object.Destroy(gameObject);
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

    [MapBuilder(Priority = Priority.LAST, Target = MapTarget.Game)]
    public static void OnPostBuild()
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