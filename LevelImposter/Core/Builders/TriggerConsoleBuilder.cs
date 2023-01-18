using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class TriggerConsoleBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-triggerconsole")
                return;

            // Prefab
            var prefab = AssetDB.GetObject("util-computer");
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

            // Sprite
            SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
            obj.layer = (int)Layer.ShortObjects;
            if (rend == null)
            {
                LILogger.Warn($"{elem.name} is missing a sprite.");
                return;
            }
            rend.material = prefabRenderer.material;

            // Spawnable Prefab
            GameObject spawnablePrefab = new GameObject(obj.name + "_Spawnable");
            LITriggerSpawnable spawnableTrigger = spawnablePrefab.AddComponent<LITriggerSpawnable>();
            Minigame spawnableGame = spawnablePrefab.AddComponent<DummyMinigame>();
            spawnableTrigger.SetTrigger(obj, "onUse");
            spawnablePrefab.SetActive(false);

            // Console
            SystemConsole console = obj.AddComponent<SystemConsole>();
            console.SafePositionLocal = new Vector2(0, 0);
            console.useIcon = ImageNames.UseButton;
            console.usableDistance = elem.properties.range == null ? 1.0f : (float)elem.properties.range;
            console.FreeplayOnly = false;
            console.onlyFromBelow = elem.properties.onlyFromBelow == true;
            console.Image = rend;
            console.MinigamePrefab = spawnableGame;


            // Colliders
            MapUtils.CreateTriggerColliders(obj, prefab);
        }

        public void PostBuild() { }
    }
}
