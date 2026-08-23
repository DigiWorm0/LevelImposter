using System;
using System.Collections.Generic;
using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Util;

internal static class UtilBuilder
{
    private const string CAM_PANEL_NAME = "Surv_Panel";
    public static List<SystemConsole> AllEmergencyButtons { get; } = [];

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        AllEmergencyButtons.Clear();
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes =
        [
            "util-button1",
            "util-button2",
            "util-cams",
            "util-cams2",
            "util-cams3",
            "util-cams4",
            "util-admin",
            "util-vitals",
            "util-computer"
        ]
    )]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;
        var prefabSystem = prefab.GetComponent<SystemConsole>();
        var prefabMap = prefab.GetComponent<MapConsole>();
        var prefabBtn = prefab.GetComponent<PassiveButton>();

        // Default Sprite
        var spriteRenderer = gameObject.CloneSprite(prefab);

        // Console
        Action action;
        if (prefabSystem != null)
        {
            var console = gameObject.AddComponent<SystemConsole>();
            console.Image = spriteRenderer;
            console.FreeplayOnly = prefabSystem.FreeplayOnly;
            console.onlyFromBelow = element.properties.onlyFromBelow == true;
            console.usableDistance = prefabSystem.usableDistance;
            console.MinigamePrefab = prefabSystem.MinigamePrefab;
            console.useIcon = prefabSystem.useIcon;
            console.usableDistance = element.properties.range ?? 1.0f;
            action = console.Use;

            // Always set minigame to polus cams
            if (element.type == "util-cams2")
                console.MinigamePrefab =
                    PrefabDB.GetObject("util-cams")?.GetComponent<SystemConsole>().MinigamePrefab;

            // Set object name for TOR Security Guard to find panel type
            if (element.type.StartsWith("util-cams"))
                gameObject.name = CAM_PANEL_NAME;

            if (element.type.StartsWith("util-button"))
                AllEmergencyButtons.Add(console);
        }
        else
        {
            // Admin Table
            var console = gameObject.AddComponent<MapConsole>();
            console.Image = spriteRenderer;
            console.useIcon = prefabMap.useIcon;
            console.usableDistance = prefabMap.usableDistance;
            console.useIcon = prefabMap.useIcon;
            if (element.properties.range != null)
                console.usableDistance = (float)element.properties.range;
            action = console.Use;
        }

        // Button
        var btn = gameObject.AddComponent<PassiveButton>();
        btn.ClickMask = prefabBtn.ClickMask;
        btn.OnMouseOver = new UnityEvent();
        btn.OnMouseOut = new UnityEvent();
        btn.OnClick.AddListener(action);

        // Colliders
        gameObject.CreateDefaultColliders(prefab);
    }
}