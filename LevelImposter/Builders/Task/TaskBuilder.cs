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

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        _shipTaskBuilder.Reset();
        _taskConsoleBuilder.Reset();
    }

    [ElementBuilder(Target = MapTarget.Game)]
    public static void BuildTask(LIBaseShip baseShip, LIElement element, GameObject gameObject)
    {
        if (!element.type.StartsWith("task-"))
            return;

        // Prefab 
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;

        // Sprite
        gameObject.CloneSprite(prefab);

        // Console
        var console = _taskConsoleBuilder.Build(element, gameObject, prefab);
        _shipTaskBuilder.BuildTask(baseShip, element, console);

        // Button
        var prefabBtn = prefab.GetComponentInChildren<PassiveButton>();
        var collider = gameObject.CreateDefaultColliders(prefab);
        if (prefabBtn != null)
        {
            var btn = gameObject.AddComponent<PassiveButton>();
            btn.ClickMask = collider;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            var action = console.Use;
            btn.OnClick.AddListener(action);
        }
    }


    [ElementBuilder(ElementTypes = ["task-medscan"])]
    public static void RegisterMedScanner(GameObject gameObject, ShipStatus shipStatus)
    {
        if (shipStatus.MedScanner != null)
            throw new MapBuildException("Only 1 med scanner can be used per map");

        shipStatus.MedScanner = gameObject.AddComponent<MedScannerBehaviour>();
    }
}