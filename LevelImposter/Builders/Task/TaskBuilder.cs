using System;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;
using LevelImposter.Core;

namespace LevelImposter.Builders
{
    public class TaskBuilder : IElemBuilder
    {
        private TaskConsoleBuilder _consoleBuilder = new();
        private ShipTaskBuilder _shipBuilder = new();

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("task-"))
                return;

            // Prefab 
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            
            // Sprite
            SpriteRenderer spriteRenderer = MapUtils.CloneSprite(obj, prefab);

            // Console
            Console console = _consoleBuilder.Build(elem, obj, prefab);
            _shipBuilder.Build(elem, console);

            // Button
            var prefabBtn = prefab.GetComponentInChildren<PassiveButton>();
            var collider = MapUtils.CreateDefaultColliders(obj, prefab);
            if (prefabBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = collider;
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Medscan
            bool isMedscan = elem.type == "task-medscan";
            if (isMedscan)
            {
                // ShipStatus
                var shipStatus = LIShipStatus.Instance?.ShipStatus;
                if (shipStatus == null)
                    throw new MissingShipException();

                // MedScanner
                if (shipStatus.MedScanner != null)
                    LILogger.Warn("Only 1 med scanner can be used per map");
                MedScannerBehaviour medscan = obj.AddComponent<MedScannerBehaviour>();
                shipStatus.MedScanner = medscan;
            }
        }

        public void PostBuild()
        {
            _consoleBuilder.PostBuild();
            _shipBuilder.PostBuild();
        }
    }
}
