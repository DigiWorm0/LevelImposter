using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class TaskBuilder : Builder
    {
        public GameObject taskContainer;
        public int consoleID = 0;

        private Dictionary<string, int> consoleIDPairs = new Dictionary<string, int> {
            { "task-garbage2", 1 },
            { "task-garbage3", 0 },
            { "task-garbage4", 2 },
            { "task-fans1", 0 },
            { "task-fans2", 1 },
        };
        private Dictionary<string, int> consoleIDIncrements = new Dictionary<string, int> {
            { "task-toilet", 0 },
            { "task-breakers", 0 },
            { "task-towels", 0 },
            { "task-node", 0 },
            { "task-waterwheel1", 0 },
            { "task-fuel2", 0 },
        };

        public static byte breakerCount = 0;
        public static byte toiletCount = 0;
        public static byte towelCount = 0;
        public static byte fuelCount = 0;
        public static byte waterWheelCount = 0;

        public static SystemTypes[] divertSystems = new SystemTypes[0];

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("task-"))
                return;

            if (taskContainer == null)
            {
                taskContainer = new GameObject("Tasks");
                taskContainer.transform.SetParent(LIShipStatus.Instance.transform);
            }

            TaskData taskData = AssetDB.tasks[elem.type];
            ShipStatus shipStatus = LIShipStatus.Instance.shipStatus;

            // Default Sprite
            obj.layer = (int)Layer.ShortObjects;
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = taskData.SpriteRenderer.sprite;
            }
            spriteRenderer.material = taskData.SpriteRenderer.material;

            // Parent
            SystemTypes systemType = 0;
            if (elem.properties.parent != null)
                systemType = RoomBuilder.GetSystem((Guid)elem.properties.parent);

            // Console
            Console console;
            Console origConsole = taskData.GameObj.GetComponent<Console>();
            if (elem.type == "task-pistols1" || elem.type == "task-rifles1")
            {
                console = obj.AddComponent<StoreArmsTaskConsole>();
                StoreArmsTaskConsole specialConsole = console.Cast<StoreArmsTaskConsole>();
                StoreArmsTaskConsole origSpecialConsole = origConsole.Cast<StoreArmsTaskConsole>();

                specialConsole.timesUsed = origSpecialConsole.timesUsed;
                specialConsole.Images = origSpecialConsole.Images;
                specialConsole.useSound = origSpecialConsole.useSound;
                specialConsole.usesPerStep = origSpecialConsole.usesPerStep;
            }
            else if (elem.type.StartsWith("task-towels") && elem.type != "task-towels1")
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
            console.ConsoleId = consoleID;
            console.Image = spriteRenderer;
            console.onlyFromBelow = elem.properties.onlyFromBelow == null ? false : (bool)elem.properties.onlyFromBelow;
            console.usableDistance = elem.properties.range == null ? 1.0f : (float)elem.properties.range;
            console.Room = systemType;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;

            if (consoleIDPairs.ContainsKey(elem.type))
            {
                console.ConsoleId = consoleIDPairs[elem.type];
            }
            else if (elem.type == "task-waterjug2")
            {
                TaskSet taskSet = new TaskSet();
                taskSet.taskType = TaskTypes.ReplaceWaterJug;
                taskSet.taskStep = new IntRange(1, 1);
                console.ValidTasks = new UnhollowerBaseLib.Il2CppReferenceArray<TaskSet>(new TaskSet[] {
                    taskSet
                });
            }
            else if (elem.type.StartsWith("task-towels"))
            {
                if (elem.type == "task-towels1")
                    console.ConsoleId = 255;
                else
                {
                    console.ConsoleId = consoleIDIncrements["task-towels"];
                    consoleIDIncrements["task-towels"]++;
                }
            }
            else if (elem.type == "task-fuel1")
            {
                console.ValidTasks = new UnhollowerBaseLib.Il2CppReferenceArray<TaskSet>(byte.MaxValue / 2);
                for (byte i = 0; i < byte.MaxValue - 1; i+=2)
                {
                    TaskSet taskSet = new TaskSet();
                    taskSet.taskType = TaskTypes.FuelEngines;
                    taskSet.taskStep = new IntRange(i, i);
                    console.ValidTasks[i / 2] = taskSet;
                }
            }
            else if (elem.type == "task-fuel2")
            {
                console.ConsoleId = consoleIDIncrements[elem.type];
                TaskSet taskSet = new TaskSet();
                taskSet.taskType = TaskTypes.FuelEngines;
                taskSet.taskStep = new IntRange(console.ConsoleId * 2 + 1, console.ConsoleId * 2 + 1);
                console.ValidTasks = new UnhollowerBaseLib.Il2CppReferenceArray<TaskSet>(new TaskSet[] {
                    taskSet
                });
                consoleIDIncrements[elem.type]++;
            }
            else if (consoleIDIncrements.ContainsKey(elem.type))
            {
                console.ConsoleId = consoleIDIncrements[elem.type];
                consoleIDIncrements[elem.type]++;
            }
            else
            {
                console.ConsoleId = consoleID;
                consoleID++;
            }

            // Collider
            if (!MapUtils.HasSolidCollider(elem))
                MapUtils.CloneColliders(origConsole.gameObject, obj);

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

            // Task
            if (elem.type == "task-divert1")
            {
                List<LIElement> divertTargets = new List<LIElement>();
                foreach (LIElement mapElem in LIShipStatus.Instance.currentMap.elements)
                    if (mapElem.type == "task-divert2")
                        divertTargets.Add(mapElem);

                divertSystems = new SystemTypes[divertTargets.Count];
                NormalPlayerTask origTask = taskData.Behavior;
                for (int i = 0; i < divertTargets.Count; i++)
                {
                    LIElement divertTarget = divertTargets[i];

                    SystemTypes divertSystem = 0;
                    if (divertTarget.properties.parent != null)
                        divertSystem = RoomBuilder.GetSystem((Guid)divertTarget.properties.parent);
                    divertSystems[i] = divertSystem;

                    GameObject taskHolder = new GameObject(elem.name);
                    taskHolder.transform.SetParent(taskContainer.transform);

                    DivertPowerTask task = taskHolder.AddComponent<DivertPowerTask>();
                    task.StartAt = systemType;
                    task.taskStep = origTask.taskStep;
                    task.MaxStep = origTask.MaxStep;
                    task.arrowSuspended = origTask.arrowSuspended;
                    task.ShowTaskTimer = origTask.ShowTaskTimer;
                    task.ShowTaskStep = origTask.ShowTaskStep;
                    task.TaskTimer = origTask.TaskTimer;
                    task.TimerStarted = origTask.TimerStarted;
                    task.TaskType = origTask.TaskType;
                    task.MinigamePrefab = origTask.MinigamePrefab;
                    task.TargetSystem = divertSystem;

                    shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task.Cast<NormalPlayerTask>());
                }
            }
            else if (!string.IsNullOrEmpty(taskData.BehaviorName))
            {
                if (!string.IsNullOrEmpty(elem.properties.description))
                    MapUtils.Rename(taskData.Behavior.TaskType, elem.properties.description);

                GameObject taskHolder = new GameObject(elem.name);
                taskHolder.transform.SetParent(taskContainer.transform);

                NormalPlayerTask origTask = taskData.Behavior;
                NormalPlayerTask task = taskHolder.AddComponent(taskData.Behavior.GetIl2CppType()).Cast<NormalPlayerTask>();
                task.StartAt = systemType;
                task.taskStep = origTask.taskStep;
                task.MaxStep = origTask.MaxStep;
                task.arrowSuspended = origTask.arrowSuspended;
                task.ShowTaskTimer = origTask.ShowTaskTimer;
                task.ShowTaskStep = origTask.ShowTaskStep;
                task.TaskTimer = origTask.TaskTimer;
                task.TimerStarted = origTask.TimerStarted;
                task.TaskType = origTask.TaskType;
                task.MinigamePrefab = origTask.MinigamePrefab;

                if (elem.type == "task-node")
                {
                    WeatherNodeTask nodeTask = task.Cast<WeatherNodeTask>();
                    nodeTask.NodeId = console.ConsoleId;
                    nodeTask.Stage2Prefab = origTask.Cast<WeatherNodeTask>().Stage2Prefab;
                }

                if (taskData.TaskType == TaskType.Common)
                    shipStatus.CommonTasks = MapUtils.AddToArr(shipStatus.CommonTasks, task);
                if (taskData.TaskType == TaskType.Short)
                    shipStatus.NormalTasks = MapUtils.AddToArr(shipStatus.NormalTasks, task);
                if (taskData.TaskType == TaskType.Long)
                    shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task);
            }

            // Medscan
            if (elem.type == "task-medscan")
            {
                if (shipStatus.MedScanner != null)
                    LILogger.Warn("Warning: Only 1 med scanner can be used per map");
                MedScannerBehaviour medscan = obj.AddComponent<MedScannerBehaviour>();
                shipStatus.MedScanner = medscan;
            }
        }

        public void PostBuild() {
            string[] keys = new string[consoleIDIncrements.Keys.Count];
            consoleIDIncrements.Keys.CopyTo(keys, 0);

            foreach (var key in keys)
            {
                byte count = (byte)consoleIDIncrements[key];
                if (key == "task-breakers")
                    breakerCount = count;
                if (key == "task-toilet")
                    toiletCount = count;
                if (key == "task-towels")
                    towelCount = count;
                if (key == "task-fuel2")
                    fuelCount = count;
                if (key == "task-waterwheel1")
                    waterWheelCount = count;
                consoleIDIncrements[key] = 0;
            }
        }

        /*
        private NormalPlayerTask SearchForTask(TaskTypes taskTypes)
        {
            ShipStatus shipStatus = LIShipStatus.Instance.shipStatus;
            foreach(var task in shipStatus.LongTasks)
                if (task.TaskType == taskTypes)
                    return task;
            foreach (var task in shipStatus.NormalTasks)
                if (task.TaskType == taskTypes)
                    return task;
            foreach (var task in shipStatus.CommonTasks)
                if (task.TaskType == taskTypes)
                    return task;
            return null;
        }
        */
    }
}
