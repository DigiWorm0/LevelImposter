using System;
using LevelImposter.Core;
using LevelImposter.Lobby;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
///     Configures the lobby map properties
/// </summary>
public class LobbyMapPropertiesBuilder : IElemBuilder
{
    public void OnPreBuild()
    {
        // Get Ship Status
        var lobby = LILobbyBehaviour.GetInstance();
        if (lobby == null)
            return;

        // Get Map
        var map = GameConfiguration.CurrentLobbyMap;
        if (map == null)
            throw new Exception("No lobby map loaded in GameConfiguration!");

        // Set Map Name
        lobby.name = map.name;

        // Set Background Color
        if (!string.IsNullOrEmpty(map.properties.bgColor) && Camera.main != null)
            if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out var bgColor))
                Camera.main.backgroundColor = bgColor;
    }
}