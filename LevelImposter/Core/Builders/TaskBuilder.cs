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

        private List<string> builtTypes = new List<string>();

        private Dictionary<string, TaskType> taskLengths = new Dictionary<string, TaskType>
        {
            { "Short", TaskType.Short },
            { "Long", TaskType.Long },
            { "Common", TaskType.Common }
        };
        private Dictionary<string, int> consoleIDPairs = new Dictionary<string, int> {
            { "task-garbage2", 1 },
            { "task-garbage3", 0 },
            { "task-garbage4", 2 },
            { "task-fans1", 0 },
            { "task-fans2", 1 },
            { "task-records1", 0 }
        };
        private Dictionary<string, int> consoleIDIncrements = new Dictionary<string, int> {
            { "task-toilet", 0 },
            { "task-breakers", 0 },
            { "task-towels", 0 },
            { "task-node", 0 },
            { "task-waterwheel1", 0 },
            { "task-fuel2", 0 },
            { "task-align1", 0 },
            { "task-records2", 1 },
            { "task-wires", 0 }
        };

        public static byte breakerCount = 0;
        public static byte toiletCount = 0;
        public static byte towelCount = 0;
        public static byte fuelCount = 0;
        public static byte waterWheelCount = 0;
        public static byte alignEngineCount = 0;
        public static byte recordsCount = 0;
        public static byte wiresCount = 0;

        public static SystemTypes[] divertSystems = new SystemTypes[0];

        private NormalPlayerTask wiresTask = null;

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("task-"))
                return;

            // Create Task Container
            if (taskContainer == null)
            {
                taskContainer = new GameObject("Tasks");
                taskContainer.transform.SetParent(LIShipStatus.Instance.transform);
            }

            // Get DB
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
            bool isBuilt = builtTypes.Contains(elem.type);
            builtTypes.Add(elem.type);

            if (elem.type == "task-divert1" && !isBuilt)
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

                    GameObject arrow = UnityEngine.Object.Instantiate(origTask.Arrow.gameObject);
                    arrow.transform.SetParent(task.transform);
                    arrow.SetActive(false);
                    task.Arrow = arrow.GetComponent<ArrowBehaviour>();

                    shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task.Cast<NormalPlayerTask>());
                }
            }
            else if (!string.IsNullOrEmpty(taskData.BehaviorName) && !isBuilt)
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

                if (origTask.Arrow != null)
                {
                    GameObject arrow = UnityEngine.Object.Instantiate(origTask.Arrow.gameObject);
                    arrow.transform.SetParent(task.transform);
                    arrow.SetActive(false);

                    task.Arrow = arrow.GetComponent<ArrowBehaviour>();
                }

                if (elem.type == "task-node")
                {
                    WeatherNodeTask nodeTask = task.Cast<WeatherNodeTask>();
                    nodeTask.NodeId = console.ConsoleId;
                    nodeTask.Stage2Prefab = origTask.Cast<WeatherNodeTask>().Stage2Prefab;
                }

                if (elem.type == "task-wires")
                {
                    wiresTask = task;
                }

                string? taskLengthProp = elem.properties.taskLength;
                TaskType taskLength = taskLengthProp != null ? taskLengths[taskLengthProp] : taskData.TaskType;
                if (taskLength == TaskType.Common)
                    shipStatus.CommonTasks = MapUtils.AddToArr(shipStatus.CommonTasks, task);
                if (taskLength == TaskType.Short)
                    shipStatus.NormalTasks = MapUtils.AddToArr(shipStatus.NormalTasks, task);
                if (taskLength == TaskType.Long)
                    shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task);
            }

            // Medscan
            if (elem.type == "task-medscan")
            {
                if (shipStatus.MedScanner != null)
                    LILogger.Warn("Only 1 med scanner can be used per map");
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
                if (key == "task-align1")
                    alignEngineCount = count;
                if (key == "task-records2")
                    recordsCount = count;
                if (key == "task-wires")
                    wiresCount = count;
                consoleIDIncrements[key] = 0;
            }

            // Wires Length
            if (wiresTask != null)
            {
                wiresTask.MaxStep = Math.Min(wiresCount, (byte)3);
            }
            wiresTask = null;
        }
    }
}
