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

            // Sprite
            SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
            obj.layer = (int)Layer.ShortObjects;
            if (rend == null)
            {
                LILogger.Warn(elem.name + " is missing a sprite.");
                return;
            }

            // Material
            UtilData fpComputerData = AssetDB.Utils["util-computer"];
            SpriteRenderer fpComputerRend = fpComputerData.GameObj.GetComponent<SpriteRenderer>();
            rend.material = fpComputerRend.material;

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

            // Collider
            PolygonCollider2D[] solidColliders = obj.GetComponentsInChildren<PolygonCollider2D>();
            for (int i = 0; i < solidColliders.Length; i++)
                solidColliders[i].isTrigger = true;
            if (solidColliders.Length <= 0)
            {
                BoxCollider2D boxCollider = obj.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(elem.xScale, elem.yScale);
                boxCollider.offset = new Vector2(elem.xScale / 2, elem.yScale / 2);
                boxCollider.isTrigger = true;
            }
        }

        public void PostBuild() { }
    }
}
