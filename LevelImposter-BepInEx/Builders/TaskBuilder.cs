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

        // Multipart Tasks
        private int nodeId = 0;
        private int divertId = 0;
        private int recordsId = 1;
        private int toiletId = 0;
        private int breakersId = 0;
        private int towelsId = 0;

        private readonly SystemTypes[] DIVERT_SYSTEMS = {
            SystemTypes.Launchpad,
            SystemTypes.MedBay,
            SystemTypes.Comms,
            SystemTypes.Office,
            SystemTypes.Laboratory,
            SystemTypes.Greenhouse,
            SystemTypes.Admin,
            SystemTypes.Cafeteria
        };

        public TaskBuilder(PolusHandler polus)
        {
            this.polus = polus;
            taskMgr = new GameObject("TaskManager");
            //taskMgr.transform.SetParent(polus.gameObject.transform);
        }

        public bool PreBuild(MapAsset asset)
        {
            if (!asset.type.StartsWith("task-"))
                return true;
            TaskData taskData = AssetDB.tasks[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = taskData.SpriteRenderer.sprite;
            spriteRenderer.material = taskData.SpriteRenderer.material;
            obj.layer = (int)Layer.ShortObjects;

            // Target Room
            SystemTypes target = 0;
            if (asset.targetIds.Length > 0)
                if (asset.targetIds[0] > 0)
                    if (ShipRoomBuilder.db.ContainsKey(asset.targetIds[0]))
                        target = ShipRoomBuilder.db[asset.targetIds[0]];

            // Divert Power
            if (asset.type == "task-divert2")
            {
                if (divertId >= DIVERT_SYSTEMS.Length)
                {
                    LILogger.LogError("Hit Divert Power's Max System Limit");
                    return false;
                }

                target = DIVERT_SYSTEMS[divertId];
                divertId++;
            }

            // Console
            Console console;
            Console origConsole = taskData.GameObj.GetComponent<Console>();
            if (asset.type == "task-pistols1" || asset.type == "task-rifles1")
            {
                console = obj.AddComponent<StoreArmsTaskConsole>();
                StoreArmsTaskConsole specialConsole = console.Cast<StoreArmsTaskConsole>();
                StoreArmsTaskConsole origSpecialConsole = origConsole.Cast<StoreArmsTaskConsole>();

                specialConsole.timesUsed = origSpecialConsole.timesUsed;
                specialConsole.Images = origSpecialConsole.Images;
                specialConsole.useSound = origSpecialConsole.useSound;
                specialConsole.usesPerStep = origSpecialConsole.usesPerStep;
            }
            else if (asset.type.StartsWith("task-towels") && asset.type != "task-towels1")
            {
                console = obj.AddComponent<TowelTaskConsole>();
                TowelTaskConsole specialConsole = console.Cast<TowelTaskConsole>();
                TowelTaskConsole origSpecialConsole = origConsole.Cast<TowelTaskConsole>();

                specialConsole.useSound = origSpecialConsole.useSound;
            }
            else
            {
                console = obj.AddComponent<Console>();
            }
            console.ConsoleId = origConsole.ConsoleId;
            console.AllowImpostor = false;
            console.checkWalls = false;
            console.GhostsIgnored = false;
            console.Image = spriteRenderer;
            console.onlyFromBelow = asset.onlyFromBottom;
            console.onlySameRoom = false;
            console.usableDistance = 1;
            console.Room       = target;
            console.TaskTypes  = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;

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

            // Multipart Tasks
            if (asset.type.StartsWith("task-waterwheel"))
            {
                int id = int.Parse(asset.type.Substring(15));
                if (1 <= id && id <= 3)
                    console.ConsoleId = id - 1;
            }
            else if (asset.type.StartsWith("task-waterjug"))
            {
                int id = int.Parse(asset.type.Substring(13));
                if (1 <= id && id <= 2)
                {
                    console.ValidTasks    = new UnhollowerBaseLib.Il2CppReferenceArray<TaskSet>(1);
                    console.ValidTasks[0] = new TaskSet();
                    console.ValidTasks[0].taskType = TaskTypes.ReplaceWaterJug;
                    console.ValidTasks[0].taskStep = new IntRange(id - 1, id - 1);
                }
            }
            else if (asset.type == "task-node")
            {
                if (nodeId >= 6)
                {
                    LILogger.LogError("Hit Weather Node's Max System Limit");
                    return false;
                }
                    
                console.ConsoleId = nodeId;
            }
            else if (asset.type == "task-records2")
            {
                if (recordsId >= 9)
                {
                    LILogger.LogError("Hit Records's Max System Limit");
                    return false;
                }

                console.ConsoleId = recordsId;
                recordsId++;
            }
            else if (asset.type == "task-toilet")
            {
                if (toiletId >= 4)
                {
                    LILogger.LogError("Hit Toilet's Max System Limit");
                    return false;
                }

                console.ConsoleId = toiletId;
                toiletId++;
            }
            else if (asset.type == "task-breakers")
            {
                if (breakersId >= 7)
                {
                    LILogger.LogError("Hit Breakers's Max System Limit");
                    return false;
                }

                console.ConsoleId = breakersId;
                breakersId++;
            }
            else if (asset.type.StartsWith("task-towels") && asset.type != "task-towels1")
            {
                if (towelsId >= 14)
                {
                    LILogger.LogError("Hit Towels's Max System Limit");
                    return false;
                }

                console.ConsoleId = towelsId;
                towelsId++;
            }

            // Task
            if (!string.IsNullOrEmpty(taskData.BehaviorName))
            {
                GameObject taskHolder = new GameObject(asset.id.ToString());
                taskHolder.transform.SetParent(taskMgr.transform);

                NormalPlayerTask origTask = taskData.Behavior;
                NormalPlayerTask task;
                if (asset.type.StartsWith("task-divert"))
                {
                    task = taskHolder.AddComponent<DivertPowerTask>();

                    DivertPowerTask taskNode = task.Cast<DivertPowerTask>();
                    DivertPowerTask origNode = origTask.Cast<DivertPowerTask>();

                    taskNode.TargetSystem = target;
                }
                else if (asset.type == "task-node")
                {
                    task = taskHolder.AddComponent<WeatherNodeTask>();

                    WeatherNodeTask taskNode = task.Cast<WeatherNodeTask>();
                    WeatherNodeTask origNode = origTask.Cast<WeatherNodeTask>();

                    taskNode.Stage2Prefab = origNode.Stage2Prefab;
                    taskNode.NodeId = nodeId;
                    nodeId++;
                }
                else if (asset.type.StartsWith("task-waterwheel"))
                {
                    task = taskHolder.AddComponent<WaterWayTask>();
                }
                else if (asset.type == "task-towels1")
                {
                    task = taskHolder.AddComponent<TowelTask>();
                }
                else
                {
                    task = taskHolder.AddComponent<NormalPlayerTask>();
                }
                task.StartAt = target;
                task.taskStep = origTask.taskStep;
                task.MaxStep = origTask.MaxStep;
                task.arrowSuspended = origTask.arrowSuspended;
                task.ShowTaskTimer = origTask.ShowTaskTimer;
                task.ShowTaskStep = origTask.ShowTaskStep;
                task.TaskTimer = origTask.TaskTimer;
                task.TimerStarted = origTask.TimerStarted;
                task.TaskType = origTask.TaskType;
                task.MinigamePrefab = origTask.MinigamePrefab;
                task.HasLocation = origTask.HasLocation;
                task.LocationDirty = origTask.LocationDirty;

                if (taskData.TaskType == TaskType.Common)
                    polus.shipStatus.CommonTasks = AssetHelper.AddToArr(polus.shipStatus.CommonTasks, task);
                if (taskData.TaskType == TaskType.Short)
                    polus.shipStatus.NormalTasks = AssetHelper.AddToArr(polus.shipStatus.NormalTasks, task);
                if (taskData.TaskType == TaskType.Long)
                    polus.shipStatus.LongTasks = AssetHelper.AddToArr(polus.shipStatus.LongTasks, task);
            }

            // Colliders
            AssetHelper.BuildColliders(asset, obj, taskData.Scale);

            // Add to Polus
            polus.shipStatus.AllConsoles = AssetHelper.AddToArr(polus.shipStatus.AllConsoles, console);
            polus.Add(obj, asset, taskData.Scale);
            return true;
        }

        public bool PostBuild()
        {
            return true;
        }
    }
}
