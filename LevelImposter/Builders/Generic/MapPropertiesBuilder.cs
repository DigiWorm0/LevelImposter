using System;
using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Generic;

/// <summary>
///     Configures the map properties
/// </summary>
internal static class MapPropertiesBuilder
{
    private static readonly Dictionary<string, string> ExileIds = new()
    {
        { "Skeld", "ss-skeld" },
        { "MiraHQ", "ss-mira" },
        { "Polus", "ss-polus" },
        { "Airship", "ss-airship" },
        { "Fungle", "ss-fungle" }
    };

    [MapBuilder(Priority = Priority.FIRST)]
    public static void ApplyMapProperties(LIMap map, LIBaseShip baseShip, ShipStatus shipStatus)
    {
        // Set Map Name
        baseShip.name = map.name;

        // Set Background Color
        if (!string.IsNullOrEmpty(map.properties.bgColor) && Camera.main != null)
            if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out var bgColor))
                Camera.main.backgroundColor = bgColor;

        // Apply Default Exile Cutscene
        if (map.MapTarget == MapTarget.Game)
            ApplyExileCutscene(map, shipStatus);
    }

    private static void ApplyExileCutscene(LIMap map, ShipStatus shipStatus)
    {
        if (string.IsNullOrEmpty(map.properties.exileID))
            return;

        if (!ExileIds.TryGetValue(map.properties.exileID, out var exileID))
            throw new Exception($"Exile ID '{map.properties.exileID}' not found in EXILE_IDS!");

        var prefabShip = PrefabDB.GetObject(exileID);
        var prefabShipStatus = prefabShip?.GetComponent<ShipStatus>();
        if (prefabShipStatus == null)
            throw new Exception($"Exile ShipStatus prefab for ID '{exileID}' not found!");

        shipStatus.ExileCutscenePrefab = prefabShipStatus.ExileCutscenePrefab;
    }
}