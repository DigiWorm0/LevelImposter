using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class UtilBuilder : Builder
    {
        private PolusHandler polus;

        public UtilBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool PreBuild(MapAsset asset)
        {
            if (!asset.type.StartsWith("util-") ||
                asset.type.StartsWith("util-vent") ||
                asset.type.StartsWith("util-spawn") ||
                asset.type == "util-room" ||
                asset.type == "util-player" ||
                asset.type == "util-cam")
                return true;

            UtilData utilData = AssetDB.utils[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            spriteRenderer.material = utilData.SpriteRenderer.material;
            obj.layer = (int)Layer.ShortObjects;

            // Console
            Action action;
            if (utilData.GameObj.GetComponent<SystemConsole>() != null)
            {
                SystemConsole origConsole = utilData.GameObj.GetComponent<SystemConsole>();
                SystemConsole console = obj.AddComponent<SystemConsole>();
                console.Image = spriteRenderer;
                console.FreeplayOnly = origConsole.FreeplayOnly;
                console.onlyFromBelow = asset.onlyFromBottom;
                console.usableDistance = origConsole.usableDistance;
                console.MinigamePrefab = origConsole.MinigamePrefab;
                if (asset.type == "util-cams2") // Convert Skeld Cams -> Polus/Airship Cams
                    console.MinigamePrefab = AssetDB.utils["util-cams"].GameObj.GetComponent<SystemConsole>().MinigamePrefab;
                console.useIcon = origConsole.useIcon;
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
                action = console.Use;
            }

            // Box Collider
            if (utilData.GameObj.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D origBox = utilData.GameObj.GetComponent<CircleCollider2D>();
                CircleCollider2D box = obj.AddComponent<CircleCollider2D>();
                box.radius = origBox.radius;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else if (utilData.GameObj.GetComponent<BoxCollider2D>() != null)
            {
                BoxCollider2D origBox = utilData.GameObj.GetComponent<BoxCollider2D>();
                BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
                box.size = origBox.size;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else if (utilData.GameObj.GetComponent<PolygonCollider2D>() != null)
            {
                PolygonCollider2D origBox = utilData.GameObj.GetComponent<PolygonCollider2D>();
                PolygonCollider2D box = obj.AddComponent<PolygonCollider2D>();
                box.points = origBox.points;
                box.pathCount = origBox.pathCount;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }

            // Button
            PassiveButton origBtn = utilData.GameObj.GetComponent<PassiveButton>();
            PassiveButton btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = origBtn.ClickMask;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            btn.OnClick.AddListener(action);

            // Colliders
            AssetHelper.BuildColliders(asset, obj);

            polus.Add(obj, asset);

            return true;
        }

        public bool PostBuild()
        {
            return true;
        }
    }
}
