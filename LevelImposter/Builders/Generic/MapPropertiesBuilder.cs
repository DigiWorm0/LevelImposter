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
    public static readonly Dictionary<string, string> EXILE_IDS = new()
    {
        { "Skeld", "ss-skeld" },
        { "MiraHQ", "ss-mira" },
        { "Polus", "ss-polus" },
        { "Airship", "ss-airship" },
        { "Fungle", "ss-fungle" }
    };

    public MapPropertiesBuilder()
    {
        // Get Ship Status
        var shipStatus = LIShipStatus.GetShip();
        if (shipStatus == null)
            return;

        // Get Map
        var map = MapLoader.CurrentMap;

        // Set Map Name
        shipStatus.name = map.name;

        // Set Background Color
        if (!string.IsNullOrEmpty(map.properties.bgColor))
            if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out var bgColor))
                Camera.main.backgroundColor = bgColor;

        // Set Exile Animation
        if (string.IsNullOrEmpty(map.properties.exileID))
            return;

        if (!EXILE_IDS.ContainsKey(map.properties.exileID))
        {
            LILogger.Warn($"Unknown exile ID: {map.properties.exileID}");
            return;
        }

        var prefabShip = AssetDB.GetObject(EXILE_IDS[map.properties.exileID]);
        var prefabShipStatus = prefabShip?.GetComponent<ShipStatus>();
        if (prefabShipStatus == null)
            return;
        shipStatus.ExileCutscenePrefab = prefabShipStatus.ExileCutscenePrefab;
    }
}