using LevelImposter.Core;
using LevelImposter.DB;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class UtilBuilder : IElemBuilder
    {
        private const string CAM_PANEL_NAME = "Surv_Panel";

        public void Build(LIElement elem, GameObject obj)
        {
            if (!(elem.type.StartsWith("util-button") ||
                elem.type.StartsWith("util-cams") ||
                elem.type == "util-admin" ||
                elem.type == "util-vitals" ||
                elem.type.StartsWith("util-button") ||
                elem.type == "util-computer"))
                return;

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
            var prefabSystem = prefab.GetComponent<SystemConsole>();
            var prefabMap = prefab.GetComponent<MapConsole>();
            var prefabBtn = prefab.GetComponent<PassiveButton>();

            // Default Sprite
            SpriteRenderer spriteRenderer = MapUtils.CloneSprite(obj, prefab);

            // Console
            Action action;
            if (prefabSystem != null)
            {
                SystemConsole console = obj.AddComponent<SystemConsole>();
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
                    console.MinigamePrefab = AssetDB.GetObject("util-cams")?.GetComponent<SystemConsole>().MinigamePrefab;

                // Set object name for TOR Security Guard to find panel type
                if (elem.type.StartsWith("util-cams"))
                    obj.name = CAM_PANEL_NAME;
            }
            else
            {
                // Admin Table
                MapConsole console = obj.AddComponent<MapConsole>();
                console.Image = spriteRenderer;
                console.useIcon = prefabMap.useIcon;
                console.usableDistance = prefabMap.usableDistance;
                console.useIcon = prefabMap.useIcon;
                if (elem.properties.range != null)
                    console.usableDistance = (float)elem.properties.range;
                action = console.Use;
            }

            // Button
            PassiveButton btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = prefabBtn.ClickMask;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            btn.OnClick.AddListener(action);

            // Colliders
            MapUtils.CreateDefaultColliders(obj, prefab);
        }

        public void PostBuild() { }
    }
}