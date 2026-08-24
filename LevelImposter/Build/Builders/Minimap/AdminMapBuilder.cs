using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Build.Builders.Util;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Build.Builders.Minimap;

internal static class AdminMapBuilder
{
    private const float ICON_OFFSET = -0.25f;

    private static readonly List<CounterArea> CounterAreaDB = [];
    private static PoolableBehavior? _poolPrefab;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        CounterAreaDB.Clear();
        _poolPrefab = null;
    }

    [ElementBuilder(
        Priority = Priority.FIRST,
        ElementTypes = ["util-room"]
    )]
    public static void Build(ShipStatus shipStatus, LIElement element)
    {
        // Check Admin
        var isAdminVisible = element.properties.isRoomAdminVisible ?? true;
        if (!isAdminVisible)
            return;

        // Minimap
        var mapBehaviour = MinimapBuilder.GetMinimap();
        var mapCountOverlay = mapBehaviour.countOverlay;

        // Prefab
        if (_poolPrefab == null)
            _poolPrefab = mapCountOverlay.CountAreas[0].pool.Prefab;

        // System
        var systemType = RoomBuilder.GetSystem(element.id);

        // Map Room
        var overlayScale = mapCountOverlay.transform.localScale.x * shipStatus.MapScale;
        GameObject roomObj = new(element.name);
        roomObj.transform.SetParent(mapCountOverlay.transform);
        roomObj.transform.localPosition = new Vector3(
            element.x * (1 / overlayScale),
            element.y * (1 / overlayScale) + ICON_OFFSET,
            -25.0f
        );

        var counterArea = roomObj.AddComponent<CounterArea>();
        counterArea.RoomType = systemType;
        counterArea.pool = roomObj.AddComponent<ObjectPoolBehavior>();
        counterArea.pool.Prefab = _poolPrefab;

        CounterAreaDB.Add(counterArea);

        mapCountOverlay.CountAreas = CounterAreaDB.ToArray();
    }

    [MapBuilder(Priority = Priority.LAST)]
    public static void OnPostBuild()
    {
        var mapBehaviour = MinimapBuilder.GetMinimap();
        var mapCountOverlay = mapBehaviour.countOverlay;

        while (mapCountOverlay.transform.childCount > CounterAreaDB.Count)
            Object.DestroyImmediate(mapCountOverlay.transform.GetChild(0).gameObject);
    }
}