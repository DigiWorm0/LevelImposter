using System;
using System.Collections.Generic;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Util;

internal class UtilBuilder : IElemBuilder
{
    private const string CAM_PANEL_NAME = "Surv_Panel";
    public static List<SystemConsole> AllEmergencyButtons { get; } = [];

    public void OnPreBuild()
    {
        AllEmergencyButtons.Clear();
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (!(elem.type.StartsWith("util-button") ||
              elem.type.StartsWith("util-cams") ||
              elem.type == "util-admin" ||
              elem.type == "util-vitals" ||
              elem.type == "util-computer"))
            return;

        // Prefab
        var prefab = PrefabDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var prefabSystem = prefab.GetComponent<SystemConsole>();
        var prefabMap = prefab.GetComponent<MapConsole>();
        var prefabBtn = prefab.GetComponent<PassiveButton>();

        // Default Sprite
        var spriteRenderer = obj.CloneSprite(prefab);

        // Console
        Action action;
        if (prefabSystem != null)
        {
            var console = obj.AddComponent<SystemConsole>();
            console.Image = spriteRenderer;
            console.FreeplayOnly = prefabSystem.FreeplayOnly;
            console.onlyFromBelow = elem.properties.onlyFromBelow == true;
            console.usableDistance = prefabSystem.usableDistance;
            console.MinigamePrefab = prefabSystem.MinigamePrefab;
            console.useIcon = prefabSystem.useIcon;
            console.usableDistance = elem.properties.range ?? 1.0f;
            action = console.Use;

            // Always set minigame to polus cams
            if (elem.type == "util-cams2")
                console.MinigamePrefab =
                    PrefabDB.GetObject("util-cams")?.GetComponent<SystemConsole>().MinigamePrefab;

            // Set object name for TOR Security Guard to find panel type
            if (elem.type.StartsWith("util-cams"))
                obj.name = CAM_PANEL_NAME;

            if (elem.type.StartsWith("util-button"))
                AllEmergencyButtons.Add(console);
        }
        else
        {
            // Admin Table
            var console = obj.AddComponent<MapConsole>();
            console.Image = spriteRenderer;
            console.useIcon = prefabMap.useIcon;
            console.usableDistance = prefabMap.usableDistance;
            console.useIcon = prefabMap.useIcon;
            if (elem.properties.range != null)
                console.usableDistance = (float)elem.properties.range;
            action = console.Use;
        }

        // Button
        var btn = obj.AddComponent<PassiveButton>();
        btn.ClickMask = prefabBtn.ClickMask;
        btn.OnMouseOver = new UnityEvent();
        btn.OnMouseOut = new UnityEvent();
        btn.OnClick.AddListener(action);

        // Colliders
        obj.CreateDefaultColliders(prefab);
    }
}