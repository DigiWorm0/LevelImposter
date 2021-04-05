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
    class SabBuilder : Builder
    {
        private PolusHandler polus;

        public SabBuilder(PolusHandler polus)
        {
            this.polus = polus;
            

        }

        public bool Build(MapAsset asset)
        {
            SabData utilData = AssetDB.sabs[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            spriteRenderer.material = utilData.SpriteRenderer.material;

            // Console
            SystemConsole origConsole = utilData.GameObj.GetComponent<SystemConsole>();
            SystemConsole console = obj.AddComponent<SystemConsole>();
            console.Image = spriteRenderer;
            console.FreeplayOnly = origConsole.FreeplayOnly;
            console.onlyFromBelow = origConsole.onlyFromBelow;
            console.usableDistance = origConsole.usableDistance;
            console.MinigamePrefab = origConsole.MinigamePrefab;
            console.useIcon = origConsole.useIcon;

            // Box Collider
            if (utilData.GameObj.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D origBox = utilData.GameObj.GetComponent<CircleCollider2D>();
                CircleCollider2D box = obj.AddComponent<CircleCollider2D>();
                box.radius = origBox.radius;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else
            {
                BoxCollider2D origBox = utilData.GameObj.GetComponent<BoxCollider2D>();
                BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
                box.size = origBox.size;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }

            // Button
            PassiveButton origBtn = utilData.GameObj.GetComponent<PassiveButton>();
            PassiveButton btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = origBtn.ClickMask;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            Action action = console.Use;
            btn.OnClick.AddListener(action);

            polus.Add(obj, asset);

            return true;
        }
    }
}
