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

            UtilData utilData = AssetDB.Utils[elem.type];

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            obj.layer = (int)Layer.ShortObjects;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            spriteRenderer.material = utilData.SpriteRenderer.material;

            // Console
            Action action;
            if (utilData.GameObj.GetComponent<SystemConsole>() != null)
            {
                SystemConsole origConsole = utilData.GameObj.GetComponent<SystemConsole>();
                SystemConsole console = obj.AddComponent<SystemConsole>();
                console.Image = obj.GetComponent<SpriteRenderer>();
                console.FreeplayOnly = origConsole.FreeplayOnly;
                console.onlyFromBelow = elem.properties.onlyFromBelow == true;
                console.usableDistance = origConsole.usableDistance;
                console.MinigamePrefab = origConsole.MinigamePrefab;
                if (elem.type == "util-cams2")
                    console.MinigamePrefab = AssetDB.Utils["util-cams"].GameObj.GetComponent<SystemConsole>().MinigamePrefab;
                console.useIcon = origConsole.useIcon;
                console.usableDistance = elem.properties.range != null ? (float)elem.properties.range : 1.0f;
                action = console.Use;
            }
            else
            {
                MapConsole origConsole = utilData.GameObj.GetComponent<MapConsole>();
                MapConsole console = obj.AddComponent<MapConsole>();
                console.Image = spriteRenderer;
                console.useIcon = origConsole.useIcon;
                console.usableDistance = origConsole.usableDistance;
                console.useIcon = origConsole.useIcon;
                if (elem.properties.range != null)
                    console.usableDistance = (float)elem.properties.range;
                action = console.Use;
            }

            // Button
            PassiveButton origBtn = utilData.GameObj.GetComponent<PassiveButton>();
            PassiveButton btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = origBtn.ClickMask;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            btn.OnClick.AddListener(action);


            // Colliders
            MapUtils.CreateTriggerColliders(obj, utilData.GameObj);
        }

        public void PostBuild() { }
    }
}