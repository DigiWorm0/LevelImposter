using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Build.Utils;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Build.Builders.Generic;

/// <summary>
///     Applied generic properties to the LIBaseShip.
/// </summary>
internal static class MapPropertiesBuilder
{
    private static readonly Dictionary<string, string> ExileIDToShipPrefabID = new()
    {
        { "Skeld", "ss-skeld" },
        { "MiraHQ", "ss-mira" },
        { "Polus", "ss-polus" },
        { "Airship", "ss-airship" },
        { "Fungle", "ss-fungle" }
    };

    [MapBuilder(Priority = Priority.FIRST)]
    public static void ApplyMapProperties(LIMap map, LIBaseShip baseShip)
    {
        // Set Map Name
        baseShip.name = map.name;

        // Set Background Color
        if (string.IsNullOrEmpty(map.properties.bgColor) || Camera.main == null)
            return;

        if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out var bgColor))
            Camera.main.backgroundColor = bgColor;
    }

    [MapBuilder(Priority = Priority.FIRST, Target = MapTarget.Game)]
    private static void ApplyExileCutscene(LIMap map, ShipStatus shipStatus)
    {
        if (string.IsNullOrEmpty(map.properties.exileID))
            return;

        if (!ExileIDToShipPrefabID.TryGetValue(map.properties.exileID, out var shipPrefabID))
            throw new MapBuildException($"Exile ID '{map.properties.exileID}' not found in EXILE_IDS!");

        var prefabShip = PrefabDB.GetObject(shipPrefabID);
        var prefabShipStatus = prefabShip?.GetComponent<ShipStatus>();
        if (prefabShipStatus == null)
            throw new MapBuildException($"Exile ShipStatus prefab for ID '{shipPrefabID}' not found!");

        shipStatus.ExileCutscenePrefab = prefabShipStatus.ExileCutscenePrefab;
    }
}