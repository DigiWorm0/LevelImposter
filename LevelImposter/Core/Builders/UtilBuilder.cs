using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class UtilBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (!(elem.type.StartsWith("util-button") ||
                elem.type.StartsWith("util-cams") ||
                elem.type == "util-admin" ||
                elem.type == "util-vitals" ||
                elem.type.StartsWith("util-button") ||
                elem.type.StartsWith("util-cams") ||
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
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            obj.layer = (int)Layer.ShortObjects;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = prefabRenderer.sprite;
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            spriteRenderer.material = prefabRenderer.material;

            // Console
            Action action;
            if (prefabSystem != null)
            {
                SystemConsole console = obj.AddComponent<SystemConsole>();
                console.Image = obj.GetComponent<SpriteRenderer>();
                console.FreeplayOnly = prefabSystem.FreeplayOnly;
                console.onlyFromBelow = elem.properties.onlyFromBelow == true;
                console.usableDistance = prefabSystem.usableDistance;
                console.MinigamePrefab = prefabSystem.MinigamePrefab;
                if (elem.type == "util-cams2")
                    console.MinigamePrefab = AssetDB.GetObject("util-cams")?.GetComponent<SystemConsole>().MinigamePrefab;
                console.useIcon = prefabSystem.useIcon;
                console.usableDistance = elem.properties.range != null ? (float)elem.properties.range : 1.0f;
                action = console.Use;
            }
            else
            {
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
            MapUtils.CreateTriggerColliders(obj, prefab);
        }

        public void PostBuild() { }
    }
}