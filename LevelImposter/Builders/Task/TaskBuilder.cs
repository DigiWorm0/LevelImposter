using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders;

public class TaskBuilder : IElemBuilder
{
    private readonly TaskConsoleBuilder _consoleBuilder = new();
    private readonly ShipTaskBuilder _shipBuilder = new();

    public void Build(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("task-"))
            return;

        // Prefab 
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;

        // Sprite
        var spriteRenderer = MapUtils.CloneSprite(obj, prefab);

        // Console
        var console = _consoleBuilder.Build(elem, obj, prefab);
        _shipBuilder.Build(elem, console);

        // Button
        var prefabBtn = prefab.GetComponentInChildren<PassiveButton>();
        var collider = MapUtils.CreateDefaultColliders(obj, prefab);
        if (prefabBtn != null)
        {
            var btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = collider;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            var action = console.Use;
            btn.OnClick.AddListener(action);
        }

        // Medscan
        var isMedscan = elem.type == "task-medscan";
        if (isMedscan)
        {
            // ShipStatus
            var shipStatus = LIShipStatus.GetInstanceOrNull()?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // MedScanner
            if (shipStatus.MedScanner != null)
                LILogger.Warn("Only 1 med scanner can be used per map");
            var medscan = obj.AddComponent<MedScannerBehaviour>();
            shipStatus.MedScanner = medscan;
        }
    }

    public void PostBuild()
    {
        _consoleBuilder.PostBuild();
        _shipBuilder.PostBuild();
    }
}