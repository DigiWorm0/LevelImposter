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
    class TaskBuilder : Builder
    {
        private PolusHandler polus;
        private GameObject taskMgr;

        public TaskBuilder(PolusHandler polus)
        {
            this.polus = polus;
            taskMgr = new GameObject("TaskManager");
            //taskMgr.transform.SetParent(polus.gameObject.transform);
        }

        public bool Build(MapAsset asset)
        {
            TaskData taskData = AssetDB.tasks[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = taskData.SpriteRenderer.sprite;
            spriteRenderer.material = taskData.SpriteRenderer.material;
            obj.layer = (int)Layer.ShortObjects;

            // Console
            Console origConsole = taskData.GameObj.GetComponent<Console>();
            Console console = obj.AddComponent<Console>();
            console.ConsoleId = origConsole.ConsoleId;
            console.AllowImpostor = false;
            console.checkWalls = false;
            console.GhostsIgnored = false;
            console.Image = spriteRenderer;
            console.onlyFromBelow = true;
            console.onlySameRoom = false;
            console.usableDistance = 1;
            console.Room       = 0;
            console.TaskTypes  = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;
            polus.Add(obj, asset);

            // Box Collider
            if (taskData.GameObj.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D origBox = taskData.GameObj.GetComponent<CircleCollider2D>();
                CircleCollider2D box = obj.AddComponent<CircleCollider2D>();
                box.radius = origBox.radius;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else if (taskData.GameObj.GetComponent<BoxCollider2D>() != null)
            {
                BoxCollider2D origBox = taskData.GameObj.GetComponent<BoxCollider2D>();
                BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
                box.size = origBox.size;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else if (taskData.GameObj.GetComponent<PolygonCollider2D>() != null)
            {
                PolygonCollider2D origBox = taskData.GameObj.GetComponent<PolygonCollider2D>();
                PolygonCollider2D box = obj.AddComponent<PolygonCollider2D>();
                box.points = origBox.points;
                box.pathCount = origBox.pathCount;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }

            // Button
            PassiveButton origBtn = taskData.GameObj.GetComponent<PassiveButton>();
            if (origBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = origBtn.ClickMask;
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Medscan
            if (asset.type == "task-medscan")
            {
                MedScannerBehaviour medscan = obj.AddComponent<MedScannerBehaviour>();
                MedScannerBehaviour origscan = taskData.GameObj.GetComponent<MedScannerBehaviour>();

                medscan.Offset = origscan.Offset;

                polus.shipStatus.MedScanner = medscan;
            }

            // Task
            if (!string.IsNullOrEmpty(taskData.BehaviorName))
            {
                NormalPlayerTask origTask = taskData.Behavior;
                NormalPlayerTask task;
                if (asset.type.StartsWith("task-divert"))
                {
                    task = taskMgr.AddComponent<DivertPowerTask>();

                    DivertPowerTask taskNode = task.Cast<DivertPowerTask>();
                    DivertPowerTask origNode = origTask.Cast<DivertPowerTask>();

                    taskNode.TargetSystem = origNode.TargetSystem;
                }
                else if (asset.type.StartsWith("task-node"))
                {
                    task = taskMgr.AddComponent<WeatherNodeTask>();

                    WeatherNodeTask taskNode = task.Cast<WeatherNodeTask>();
                    WeatherNodeTask origNode = origTask.Cast<WeatherNodeTask>();

                    taskNode.Stage2Prefab = origNode.Stage2Prefab;
                    taskNode.NodeId = origNode.NodeId;
                }
                else
                {
                    task = taskMgr.AddComponent<NormalPlayerTask>();
                }
                task.taskStep = origTask.taskStep;
                task.MaxStep = origTask.MaxStep;
                task.arrowSuspended = origTask.arrowSuspended;
                task.ShowTaskTimer = origTask.ShowTaskTimer;
                task.ShowTaskStep = origTask.ShowTaskStep;
                task.TaskTimer = origTask.TaskTimer;
                task.TimerStarted = origTask.TimerStarted;
                task.StartAt = 0;
                task.TaskType = origTask.TaskType;
                task.MinigamePrefab = origTask.MinigamePrefab;
                task.HasLocation = origTask.HasLocation;
                task.LocationDirty = origTask.LocationDirty;

                if (taskData.TaskType == TaskType.Common)
                    polus.shipStatus.CommonTasks = AssetBuilder.AddToArr(polus.shipStatus.CommonTasks, task);
                if (taskData.TaskType == TaskType.Short)
                    polus.shipStatus.NormalTasks = AssetBuilder.AddToArr(polus.shipStatus.NormalTasks, task);
                if (taskData.TaskType == TaskType.Long)
                    polus.shipStatus.LongTasks = AssetBuilder.AddToArr(polus.shipStatus.LongTasks, task);
            }

            // Colliders
            AssetBuilder.BuildColliders(asset, obj);

            // Add to Polus
            polus.shipStatus.AllConsoles = AssetBuilder.AddToArr(polus.shipStatus.AllConsoles, console);
            return true;
        }
    }
}
