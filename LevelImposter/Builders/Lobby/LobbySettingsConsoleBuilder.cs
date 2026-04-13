using System;
using LevelImposter.Core;
using LevelImposter.Lobby;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Lobby;

public class LobbySettingsConsoleBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject gameObject)
    {
        if (elem.type != "util-lobbysettings")
            return;

        // Load Prefab
        var prefab = LobbyDropshipPrefab.GetObjectFromPrefab("SmallBox/Panel");
        var prefabConsole = prefab.GetComponent<OptionsConsole>();
        var prefabButton = prefab.GetComponent<PassiveButton>();

        // Build Console
        var console = gameObject.AddComponent<OptionsConsole>();
        console.CustomPosition = prefabConsole.CustomPosition;
        console.HostOnly = true;
        console.MenuPrefab = prefabConsole.MenuPrefab;
        console.Outline = MapUtils.CloneSprite(gameObject, prefab);
        console.CustomUseIcon = ImageNames.OptionsButton;

        // Button
        var button = gameObject.AddComponent<PassiveButton>();
        button.ClickMask = prefabButton?.ClickMask;
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick.AddListener((Action)console.Use);

        // Colliders
        MapUtils.CreateDefaultColliders(gameObject, prefab);
    }
}