using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
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

        public SabBuilder(PolusHandler polus)
        {
            this.polus = polus;
            sabMgr = new GameObject("SabManager");
        }

        public bool Build(MapAsset asset)
        {
            LILogger.LogError("Sabotages are not yet supported!");
            MinimapGen.SabGenerator.AddSabotage(asset);
            return false;

            /*
            SabData sabData = AssetDB.sabs[asset.type];

            // Object
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
            console.AllowImpostor = false;
            console.checkWalls = false;
            console.GhostsIgnored = false;
            console.Image = spriteRenderer;
            console.onlyFromBelow = true;
            console.onlySameRoom = false;
            console.usableDistance = 1;
            //console.Room = origConsole.Room;
            console.Room = SystemTypes.Electrical;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;
            polus.Add(obj, asset);

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

            polus.shipStatus.AllConsoles = AssetBuilder.AddToArr(polus.shipStatus.AllConsoles, console);

            // Target Room
            SystemTypes target = 0;
            if (asset.targetIds.Length <= 0)
                LILogger.LogWarn(asset.name + " has no target room");
            else
                target = ShipRoomBuilder.db[asset.targetIds[0]];
            

            // Special Task
            if (asset.type == "sab-electric")
            {
                ElectricTask task = sabMgr.AddComponent<ElectricTask>();
                ElectricTask origTask = sabData.Behavior.Cast<ElectricTask>();

                task.even = origTask.even;
                task.isComplete = origTask.isComplete;
                task.didContribute = origTask.didContribute;
                task.Id = origTask.Id;
                task.Index = origTask.Index;
                task.LocationDirty = origTask.LocationDirty;
                task.HasLocation = origTask.HasLocation;
                task.MinigamePrefab = origTask.MinigamePrefab;
                task.TaskType = origTask.TaskType;
                task.StartAt = target;
                task.system = new SwitchSystem();

                polus.shipStatus.SpecialTasks = AssetBuilder.AddToArr(polus.shipStatus.SpecialTasks, task);

                List<StringNames> list = new List<StringNames>(polus.shipStatus.SystemNames);
                list.Add(StringNames.FixLights);
                polus.shipStatus.SystemNames = list.ToArray();
                
            }
            else
            {
                throw new Exception();
            }

            // Colliders
            AssetBuilder.BuildColliders(asset, obj);

            polus.Add(obj, asset);

            return true;
            */
        }
    }
}
