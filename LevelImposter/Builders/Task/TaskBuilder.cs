using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Task;

public static class TaskBuilder
{
    private static readonly ShipTaskBuilder _shipTaskBuilder = new();
    private static readonly TaskConsoleBuilder _taskConsoleBuilder = new();

    [MapBuilder(Target = MapTarget.Game)]
    public static void BuildTask(LIBaseShip baseShip, LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("task-"))
            return;

        // Prefab 
        var prefab = PrefabDB.GetObject(elem.type);
        if (prefab == null)
            return;

        // Sprite
        obj.CloneSprite(prefab);

        // Console
        var console = _taskConsoleBuilder.Build(elem, obj, prefab);
        _shipTaskBuilder.BuildTask(baseShip, elem, console);

        // Button
        var prefabBtn = prefab.GetComponentInChildren<PassiveButton>();
        var collider = obj.CreateDefaultColliders(prefab);
        if (prefabBtn != null)
        {
            var btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = collider;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            var action = console.Use;
            btn.OnClick.AddListener(action);
        }
    }


    [ElementBuilder(ElementTypes = ["task-medscan"])]
    public static void RegisterMedScanner(GameObject gameObject)
    {
        if (ShipStatus.Instance.MedScanner != null)
            throw new MapBuildException("Only 1 med scanner can be used per map");

        ShipStatus.Instance.MedScanner = gameObject.AddComponent<MedScannerBehaviour>();
    }
}