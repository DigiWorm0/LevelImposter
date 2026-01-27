using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders;

public class SabConsoleBuilder : IElemBuilder
{
    private static readonly Dictionary<string, int> ConsoleIDPairs = new()
    {
        { "sab-electric", 0 },
        { "sab-reactorleft", 0 },
        { "sab-reactorright", 1 },
        { "sab-oxygen1", 0 },
        { "sab-oxygen2", 1 },
        { "sab-comms", 0 }
    };

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("sab-") || elem.type.StartsWith("sab-btn") || elem.type.StartsWith("sab-door"))
            return;

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var prefabConsole = prefab.GetComponent<Console>();

        // Default Sprite
        var spriteRenderer = MapUtils.CloneSprite(obj, prefab);

        // Parent
        var systemType = RoomBuilder.GetParentOrDefault(elem);
        var isFound = SabBuilder.TryGetSabotage(systemType, out var sabotageTask);
        if (!isFound || sabotageTask == null)
        {
            LILogger.Warn($"SabotageTask not found for {obj.name}");
            return;
        }

        if (!string.IsNullOrEmpty(elem.properties.description))
            LIBaseShip.Instance?.Renames.Add(sabotageTask.TaskType, elem.properties.description);

        // Console
        var console = obj.AddComponent<Console>();
        console.ConsoleId = 0;
        console.Image = spriteRenderer;
        console.onlyFromBelow = elem.properties.onlyFromBelow ?? false;
        console.usableDistance = elem.properties.range ?? 1.0f;
        console.Room = systemType;
        console.TaskTypes = prefabConsole.TaskTypes;
        console.ValidTasks = prefabConsole.ValidTasks;
        console.AllowImpostor = true;
        console.GhostsIgnored = true;
        console.checkWalls = elem.properties.checkCollision ?? false;

        if (ConsoleIDPairs.ContainsKey(elem.type))
            console.ConsoleId = ConsoleIDPairs[elem.type];

        // Colliders
        MapUtils.CreateDefaultColliders(obj, prefab);

        // Button
        var origBtn = prefab.GetComponent<PassiveButton>();
        if (origBtn != null)
        {
            var btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = obj.GetComponent<Collider2D>();
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            var action = console.Use;
            btn.OnClick.AddListener(action);
        }

        // Arrow
        var arrow = MakeArrow(sabotageTask.transform, $"{elem.name} Arrow");
        if (arrow != null)
            sabotageTask.Arrows = MapUtils.AddToArr(sabotageTask.Arrows, arrow);
    }

    /// <summary>
    ///     Builds a sabotage arrow
    /// </summary>
    /// <param name="parent">Parent object to attatch to</param>
    /// <param name="name">Name of the arrows</param>
    /// <returns>ArrowBehaviour to append to SabotageTask</returns>
    private static ArrowBehaviour? MakeArrow(Transform parent, string name)
    {
        // Prefab
        var prefab = AssetDB.GetTask<PlayerTask>("sab-comms");
        if (prefab == null)
            return null;
        var prefabArrow = prefab.gameObject.transform.FindChild("Arrow").gameObject;
        var prefabArrowRenderer = prefabArrow.GetComponent<SpriteRenderer>();

        // Object
        GameObject arrowObj = new(name);

        // Sprite
        var arrowSprite = arrowObj.AddComponent<SpriteRenderer>();
        arrowSprite.sprite = prefabArrowRenderer.sprite;
        arrowSprite.material = prefabArrowRenderer.material;
        arrowObj.layer = (int)Layer.UI;

        // Arrow Behaviour
        var arrowBehaviour = arrowObj.AddComponent<ArrowBehaviour>();
        arrowBehaviour.image = arrowSprite;

        // Transform
        arrowObj.transform.SetParent(parent);
        arrowObj.active = false;

        return arrowBehaviour;
    }
}