using System;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Builders;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Lobby;

internal static class LobbyWardrobeConsoleBuilder
{
    [ElementBuilder(
        Target = MapTarget.Lobby,
        ElementTypes = ["util-lobbywardrobe"]
    )]
    public static void Build(GameObject gameObject)
    {
        // Load Prefab
        var prefab = LobbyDropshipPrefab.GetObjectFromPrefab("panel_Wardrobe/Console");
        var prefabConsole = prefab.GetComponent<OptionsConsole>();

        // Build Console
        var console = gameObject.AddComponent<OptionsConsole>();
        console.CustomPosition = prefabConsole.CustomPosition;
        console.HostOnly = false;
        console.MenuPrefab = prefabConsole.MenuPrefab;
        console.Outline = gameObject.CloneSprite(prefab.transform.parent.gameObject);
        console.CustomUseIcon = ImageNames.WardrobeButton;

        // Button
        var button = gameObject.AddComponent<ButtonBehavior>();
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick.AddListener((Action)console.Use);

        // Colliders
        gameObject.CreateDefaultColliders(prefab);
    }
}