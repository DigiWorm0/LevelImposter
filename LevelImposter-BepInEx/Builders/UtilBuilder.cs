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

        public bool Build(MapAsset asset)
        {
            AssetData original = AssetDB.Get(asset.data);

            // Object
            GameObject obj = new GameObject(asset.data);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = original.spriteRenderer.sprite;
            spriteRenderer.material = original.spriteRenderer.material;

            // Console
            SystemConsole origConsole = original.mapObj.GetComponent<SystemConsole>();
            SystemConsole console = obj.AddComponent<SystemConsole>();
            console.Image = spriteRenderer;
            console.FreeplayOnly = origConsole.FreeplayOnly;
            console.onlyFromBelow = origConsole.onlyFromBelow;
            console.usableDistance = origConsole.usableDistance;
            console.MinigamePrefab = origConsole.MinigamePrefab;
            console.useIcon = origConsole.useIcon;

            // Box Collider
            if (original.mapObj.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D origBox = original.mapObj.GetComponent<CircleCollider2D>();
                CircleCollider2D box = obj.AddComponent<CircleCollider2D>();
                box.radius = origBox.radius;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else
            {
                BoxCollider2D origBox = original.mapObj.GetComponent<BoxCollider2D>();
                BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
                box.size = origBox.size;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }

            // Button
            PassiveButton origBtn = original.mapObj.GetComponent<PassiveButton>();
            PassiveButton btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = origBtn.ClickMask;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            Action action = console.Use;
            btn.OnClick.AddListener(action);

            polus.Add(obj, asset);

            return obj;
        }
    }
}
