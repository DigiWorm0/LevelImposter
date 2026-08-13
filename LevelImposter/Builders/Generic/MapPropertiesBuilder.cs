using System;
using System.Collections.Generic;
using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Generic;

/// <summary>
///     Configures the map properties
/// </summary>
public class MapPropertiesBuilder : IElemBuilder
{
    private static readonly Dictionary<string, string> ExileIds = new()
    {
        { "Skeld", "ss-skeld" },
        { "MiraHQ", "ss-mira" },
        { "Polus", "ss-polus" },
        { "Airship", "ss-airship" },
        { "Fungle", "ss-fungle" }
    };

    public void OnPreBuild()
    {
        // Get Ship Status
        var shipStatus = LIShipStatus.GetShip();
        if (shipStatus == null)
            return;

        // Get Map
        var map = GameConfiguration.CurrentMap;
        if (map == null)
            throw new Exception("No map loaded in GameConfiguration!");

        // Set Map Name
        shipStatus.name = map.name;

        // Set Background Color
        if (!string.IsNullOrEmpty(map.properties.bgColor) && Camera.main != null)
            if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out var bgColor))
                Camera.main.backgroundColor = bgColor;

        // Set Exile Animation
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