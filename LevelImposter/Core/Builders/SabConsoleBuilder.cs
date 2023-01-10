using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class SabConsoleBuilder : IElemBuilder
    {
        public static readonly Dictionary<string, int> CONSOLE_ID_PAIRS = new() {
            { "sab-electric", 0 },
            { "sab-reactorleft", 0 },
            { "sab-reactorright", 1 },
            { "sab-oxygen1", 0 },
            { "sab-oxygen2", 1 },
            { "sab-comms", 0 },
        };

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("sab-") || elem.type.StartsWith("sab-btn") || elem.type.StartsWith("sab-door"))
                return;
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");

            SabData sabData = AssetDB.Sabs[elem.type];
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;
            SabotageTask sabClone = sabData.Behavior.Cast<SabotageTask>();


            // Default Sprite
            obj.layer = (int)Layer.ShortObjects;
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sabData.SpriteRenderer.sprite;
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            spriteRenderer.material = sabData.SpriteRenderer.material;

            // Parent
            SystemTypes systemType = RoomBuilder.GetParentOrDefault(elem);
            bool isFound = SabBuilder.TryGetSabotage(systemType, out SabotageTask? sabotageTask);
            if (!isFound || sabotageTask == null)
            {
                LILogger.Warn($"SabotageTask not found for {obj.name}");
                return;
            }
            if (!string.IsNullOrEmpty(elem.properties.description))
                MapUtils.Rename(sabotageTask.TaskType, elem.properties.description);

            // Console
            Console console = obj.AddComponent<Console>();
            Console origConsole = sabData.GameObj.GetComponent<Console>();
            console.ConsoleId = 0;
            console.Image = spriteRenderer;
            console.onlyFromBelow = elem.properties.onlyFromBelow == null ? true : (bool)elem.properties.onlyFromBelow;
            console.usableDistance = elem.properties.range == null ? 1.0f : (float)elem.properties.range;
            console.Room = systemType;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;
            console.AllowImpostor = true;
            console.GhostsIgnored = true;

            if (CONSOLE_ID_PAIRS.ContainsKey(elem.type))
                console.ConsoleId = CONSOLE_ID_PAIRS[elem.type];

            // Button
            PolygonCollider2D collider = obj.AddComponent<PolygonCollider2D>();
            collider.isTrigger = true;
            PassiveButton origBtn = sabData.GameObj.GetComponent<PassiveButton>();
            if (origBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = collider;
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Arrow
            ArrowBehaviour arrow = MakeArrow(sabotageTask.transform, $"{elem.name} Arrow");
            sabotageTask.Arrows = MapUtils.AddToArr(sabotageTask.Arrows, arrow);
        }

        public void PostBuild() { }

        /// <summary>
        /// Builds a sabotage arrow
        /// </summary>
        /// <param name="parent">Parent object to attatch to</param>
        /// <param name="name">Name of the arrows</param>
        /// <returns>ArrowBehaviour to append to SabotageTask</returns>
        private ArrowBehaviour MakeArrow(Transform parent, string name)
        {
            // Arrow Buttons
            GameObject arrowClone = AssetDB.Sabs["sab-comms"].Behavior.gameObject.transform.FindChild("Arrow").gameObject;
            SpriteRenderer arrowCloneSprite = arrowClone.GetComponent<SpriteRenderer>();
            GameObject arrowObj = new(name);

            // Sprite
            SpriteRenderer arrowSprite = arrowObj.AddComponent<SpriteRenderer>();
            arrowSprite.sprite = arrowCloneSprite.sprite;
            arrowSprite.material = arrowCloneSprite.material;
            arrowObj.layer = (int)Layer.UI;

            // Arrow Behaviour
            ArrowBehaviour arrowBehaviour = arrowObj.AddComponent<ArrowBehaviour>();
            arrowBehaviour.image = arrowSprite;

            // Transform
            arrowObj.transform.SetParent(parent);
            arrowObj.active = false;

            return arrowBehaviour;
        }
    }
}
