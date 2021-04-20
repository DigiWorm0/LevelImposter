using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.MinimapGen;
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
        private GameObject sabMgr;
        private ArrowBehaviour sabArrow;

        public SabBuilder(PolusHandler polus)
        {
            this.polus = polus;
            sabMgr = new GameObject("SabManager");
            MakeArrow();
        }

        private void MakeArrow()
        {
            // Arrow Buttons
            GameObject arrowClone = AssetDB.sabs["sab-comms"].Behavior.gameObject.transform.FindChild("Arrow").gameObject;
            SpriteRenderer arrowCloneSprite = arrowClone.GetComponent<SpriteRenderer>();
            GameObject arrowObj = new GameObject("Sabotage Arrow");

            // Sprite
            SpriteRenderer arrowSprite = arrowObj.AddComponent<SpriteRenderer>();
            arrowSprite.sprite = arrowCloneSprite.sprite;
            arrowSprite.material = arrowCloneSprite.material;
            arrowObj.layer = (int)Layer.UI;

            // Arrow Behaviour
            ArrowBehaviour arrowBehaviour = arrowObj.AddComponent<ArrowBehaviour>();
            arrowBehaviour.image = arrowSprite;
            sabArrow = arrowBehaviour;

            // Transform
            arrowObj.transform.SetParent(sabMgr.transform);
            arrowObj.transform.localScale = new Vector3(0.4f, 0.4f, 1.0f);
            arrowObj.active = false;
        }

        public bool Build(MapAsset asset)
        {
            // System Type
            if (!SabGenerator.SABOTAGE_IDS.ContainsKey(asset.type))
            {
                LILogger.LogInfo(SabGenerator.SABOTAGE_IDS.Count);
                return false;
            }
            SystemTypes sys = SabGenerator.SABOTAGE_IDS[asset.type];

            // GameObject
            SabData sabData = AssetDB.sabs[asset.type];
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sabData.SpriteRenderer.sprite;
            spriteRenderer.material = sabData.SpriteRenderer.material;
            obj.layer = (int)Layer.ShortObjects;

            // Console
            Console origConsole = sabData.GameObj.GetComponent<Console>();
            Console console = obj.AddComponent<Console>();
            console.ConsoleId = origConsole.ConsoleId;
            console.AllowImpostor = true;
            console.checkWalls = false;
            console.GhostsIgnored = true;
            console.Image = spriteRenderer;
            console.onlyFromBelow = false;
            console.onlySameRoom = false;
            console.usableDistance = 1;
            console.Room = sys;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;

            polus.shipStatus.AllConsoles = AssetBuilder.AddToArr(polus.shipStatus.AllConsoles, console);

            // Box Collider
            if (sabData.GameObj.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D origBox = sabData.GameObj.GetComponent<CircleCollider2D>();
                CircleCollider2D box = obj.AddComponent<CircleCollider2D>();
                box.radius = origBox.radius;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else if (sabData.GameObj.GetComponent<BoxCollider2D>() != null)
            {
                BoxCollider2D origBox = sabData.GameObj.GetComponent<BoxCollider2D>();
                BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
                box.size = origBox.size;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else if (sabData.GameObj.GetComponent<PolygonCollider2D>() != null)
            {
                PolygonCollider2D origBox = sabData.GameObj.GetComponent<PolygonCollider2D>();
                PolygonCollider2D box = obj.AddComponent<PolygonCollider2D>();
                box.points = origBox.points;
                box.pathCount = origBox.pathCount;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }

            if (asset.type == "sab-comms")
            {
                HudOverrideTask task = sabMgr.AddComponent<HudOverrideTask>();
                HudOverrideTask origTask = sabData.Behavior.Cast<HudOverrideTask>();
                
                task.Arrows = new UnhollowerBaseLib.Il2CppReferenceArray<ArrowBehaviour>(1);
                task.Arrows[0] = sabArrow;
                
                task.even = origTask.even;
                task.isComplete = origTask.isComplete;
                task.didContribute = origTask.didContribute;
                task.Id = origTask.Id;
                task.Index = origTask.Index;
                task.LocationDirty = origTask.LocationDirty;
                task.HasLocation = origTask.HasLocation;
                task.MinigamePrefab = origTask.MinigamePrefab;
                task.TaskType = origTask.TaskType;
                task.StartAt = sys;
                task.system = polus.shipStatus.Systems[sys].Cast<HudOverrideSystemType>();

                polus.shipStatus.SpecialTasks = AssetBuilder.AddToArr(polus.shipStatus.SpecialTasks, task);

                List<StringNames> list = new List<StringNames>(polus.shipStatus.SystemNames);
                list.Add(StringNames.FixComms);
                polus.shipStatus.SystemNames = list.ToArray();
            }

            // Add to Polus
            AssetBuilder.BuildColliders(asset, obj);
            MinimapGen.SabGenerator.AddSabotage(asset);
            polus.Add(obj, asset);

            return true;
        }
    }
}
