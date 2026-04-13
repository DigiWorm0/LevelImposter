using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Builders;

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
            throw new System.Exception("No map loaded in GameConfiguration!");

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
            throw new System.Exception($"Exile ID '{map.properties.exileID}' not found in EXILE_IDS!");

        var prefabShip = AssetDB.GetObject(exileID);
        var prefabShipStatus = prefabShip?.GetComponent<ShipStatus>();
        if (prefabShipStatus == null)
            throw new System.Exception($"Exile ShipStatus prefab for ID '{exileID}' not found!");
        
        shipStatus.ExileCutscenePrefab = prefabShipStatus.ExileCutscenePrefab;
    }
}