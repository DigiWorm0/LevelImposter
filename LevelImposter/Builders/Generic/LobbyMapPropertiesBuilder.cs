using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Lobby.Components;
using UnityEngine;

namespace LevelImposter.Builders.Generic;

/// <summary>
///     Configures the lobby map properties
/// </summary>
public static class LobbyMapPropertiesBuilder
{
    [MapBuilder(Target = MapTarget.Lobby, Priority = Priority.FIRST)]
    public static void ApplyMapProperties(LIMap map)
    {
        // Get Ship Status
        var lobby = LILobbyBehaviour.GetInstance();
        if (lobby == null)
            return;

        // Set Map Name
        lobby.name = map.name;

        // Set Background Color
        if (!string.IsNullOrEmpty(map.properties.bgColor) && Camera.main != null)
            if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out var bgColor))
                Camera.main.backgroundColor = bgColor;
    }
}