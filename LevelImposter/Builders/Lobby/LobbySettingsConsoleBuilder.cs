using System;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Builders;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Lobby;

internal static class LobbySettingsConsoleBuilder
{
    [ElementBuilder(
        Target = MapTarget.Lobby,
        ElementTypes = ["util-lobbysettings"]
    )]
    public static void Build(LIElement elem, GameObject gameObject)
    {
        // Load Prefab
        var prefab = LobbyDropshipPrefab.GetObjectFromPrefab("SmallBox/Panel");
        var prefabConsole = prefab.GetComponent<OptionsConsole>();
        var prefabButton = prefab.GetComponent<PassiveButton>();

        // Build Console
        var console = gameObject.AddComponent<OptionsConsole>();
        console.CustomPosition = prefabConsole.CustomPosition;
        console.HostOnly = true;
        console.MenuPrefab = prefabConsole.MenuPrefab;
        console.Outline = gameObject.CloneSprite(prefab);
        console.CustomUseIcon = ImageNames.OptionsButton;

        // Button
        var button = gameObject.AddComponent<PassiveButton>();
        button.ClickMask = prefabButton?.ClickMask;
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick.AddListener((Action)console.Use);

        // Colliders
        gameObject.CreateDefaultColliders(prefab);
    }
}