using LevelImposter.Core;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
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

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabConsole = prefab.GetComponent<Console>();

            // Default Sprite
            SpriteRenderer spriteRenderer = MapUtils.CloneSprite(obj, prefab);

            // Parent
            SystemTypes systemType = RoomBuilder.GetParentOrDefault(elem);
            bool isFound = SabBuilder.TryGetSabotage(systemType, out SabotageTask? sabotageTask);
            if (!isFound || sabotageTask == null)
            {
                LILogger.Warn($"SabotageTask not found for {obj.name}");
                return;
            }
            if (!string.IsNullOrEmpty(elem.properties.description))
                LIShipStatus.Instance?.Renames.Add(sabotageTask.TaskType, elem.properties.description);

            // Console
            Console console = obj.AddComponent<Console>();
            console.ConsoleId = 0;
            console.Image = spriteRenderer;
            console.onlyFromBelow = elem.properties.onlyFromBelow ?? true;
            console.usableDistance = elem.properties.range ?? 1.0f;
            console.Room = systemType;
            console.TaskTypes = prefabConsole.TaskTypes;
            console.ValidTasks = prefabConsole.ValidTasks;
            console.AllowImpostor = true;
            console.GhostsIgnored = true;
            console.checkWalls = elem.properties.checkCollision ?? false;

            if (CONSOLE_ID_PAIRS.ContainsKey(elem.type))
                console.ConsoleId = CONSOLE_ID_PAIRS[elem.type];

            // Colliders
            MapUtils.CreateDefaultColliders(obj, prefab);

            // Button
            PassiveButton origBtn = prefab.GetComponent<PassiveButton>();
            if (origBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = obj.GetComponent<Collider2D>();
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Arrow
            ArrowBehaviour? arrow = MakeArrow(sabotageTask.transform, $"{elem.name} Arrow");
            if (arrow != null)
                sabotageTask.Arrows = MapUtils.AddToArr(sabotageTask.Arrows, arrow);
        }

        public void PostBuild() { }

        /// <summary>
        /// Builds a sabotage arrow
        /// </summary>
        /// <param name="parent">Parent object to attatch to</param>
        /// <param name="name">Name of the arrows</param>
        /// <returns>ArrowBehaviour to append to SabotageTask</returns>
        private ArrowBehaviour? MakeArrow(Transform parent, string name)
        {
            // Prefab
            var prefab = AssetDB.GetTask<PlayerTask>("sab-comms");
            if (prefab == null)
                return null;
            GameObject prefabArrow = prefab.gameObject.transform.FindChild("Arrow").gameObject;
            SpriteRenderer prefabArrowRenderer = prefabArrow.GetComponent<SpriteRenderer>();

            // Object
            GameObject arrowObj = new(name);

            // Sprite
            SpriteRenderer arrowSprite = arrowObj.AddComponent<SpriteRenderer>();
            arrowSprite.sprite = prefabArrowRenderer.sprite;
            arrowSprite.material = prefabArrowRenderer.material;
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
