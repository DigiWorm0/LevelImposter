using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class TaskBuilder : IElemBuilder
    {
        private GameObject _taskContainer;
        private int _consoleID = 0;
        private List<string> _builtTypes = new List<string>();
        private NormalPlayerTask _wiresTask = null;
        private Dictionary<string, TaskType> _taskLengths = new Dictionary<string, TaskType>
        {
            { "Short", TaskType.Short },
            { "Long", TaskType.Long },
            { "Common", TaskType.Common }
        };
        private Dictionary<string, int> _consoleIDPairs = new Dictionary<string, int> {
            { "task-garbage2", 1 },
            { "task-garbage3", 0 },
            { "task-garbage4", 2 },
            { "task-fans1", 0 },
            { "task-fans2", 1 },
            { "task-records1", 0 }
        };
        private Dictionary<string, int> _consoleIDIncrements = new Dictionary<string, int> {
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

        public static SystemTypes[] DivertSystems = new SystemTypes[0];
        public static byte BreakerCount = 0;
        public static byte ToiletCount = 0;
        public static byte TowelCount = 0;
        public static byte FuelCount = 0;
        public static byte WaterWheelCount = 0;
        public static byte AlignEngineCount = 0;
        public static byte RecordsCount = 0;
        public static byte WiresCount = 0;

        public TaskBuilder()
        {
            DivertSystems = new SystemTypes[0];
            BreakerCount = 0;
            ToiletCount = 0;
            TowelCount = 0;
            FuelCount = 0;
            WaterWheelCount = 0;
            AlignEngineCount = 0;
            RecordsCount = 0;
            WiresCount = 0;
    }

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("task-"))
                return;

            // Create Task Container
            if (_taskContainer == null)
            {
                _taskContainer = new GameObject("Tasks");
                _taskContainer.transform.SetParent(LIShipStatus.Instance.transform);
            }

            // Get DB
            TaskData taskData = AssetDB.Tasks[elem.type];
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;

            // Default Sprite
            obj.layer = (int)Layer.ShortObjects;
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = taskData.SpriteRenderer.sprite;
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
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
            console.ConsoleId = _consoleID;
            console.Image = spriteRenderer;
            console.onlyFromBelow = elem.properties.onlyFromBelow == null ? false : (bool)elem.properties.onlyFromBelow;
            console.usableDistance = elem.properties.range == null ? 1.0f : (float)elem.properties.range;
            console.Room = systemType;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;

            if (_consoleIDPairs.ContainsKey(elem.type))
            {
                console.ConsoleId = _consoleIDPairs[elem.type];
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
                    console.ConsoleId = _consoleIDIncrements["task-towels"];
                    _consoleIDIncrements["task-towels"]++;
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
                console.ConsoleId = _consoleIDIncrements[elem.type];
                TaskSet taskSet = new TaskSet();
                taskSet.taskType = TaskTypes.FuelEngines;
                taskSet.taskStep = new IntRange(console.ConsoleId * 2 + 1, console.ConsoleId * 2 + 1);
                console.ValidTasks = new UnhollowerBaseLib.Il2CppReferenceArray<TaskSet>(new TaskSet[] {
                    taskSet
                });
                _consoleIDIncrements[elem.type]++;
            }
            else if (_consoleIDIncrements.ContainsKey(elem.type))
            {
                console.ConsoleId = _consoleIDIncrements[elem.type];
                _consoleIDIncrements[elem.type]++;
            }
            else
            {
                console.ConsoleId = _consoleID;
                _consoleID++;
            }

            // Button
            PolygonCollider2D collider = obj.AddComponent<PolygonCollider2D>();
            collider.isTrigger = true;
            PassiveButton origBtn = taskData.GameObj.GetComponent<PassiveButton>();
            if (origBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = collider;
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Task
            bool isBuilt = _builtTypes.Contains(elem.type);
            _builtTypes.Add(elem.type);

            if (!isBuilt)
                LILogger.Info("Adding task for " + elem.name + "...");

            if (elem.type == "task-divert1" && !isBuilt)
            {
                List<LIElement> divertTargets = new List<LIElement>();
                foreach (LIElement mapElem in LIShipStatus.Instance.CurrentMap.elements)
                    if (mapElem.type == "task-divert2")
                        divertTargets.Add(mapElem);

                DivertSystems = new SystemTypes[divertTargets.Count];
                NormalPlayerTask origTask = taskData.Behavior;
                for (int i = 0; i < divertTargets.Count; i++)
                {
                    LIElement divertTarget = divertTargets[i];

                    SystemTypes divertSystem = 0;
                    if (divertTarget.properties.parent != null)
                        divertSystem = RoomBuilder.GetSystem((Guid)divertTarget.properties.parent);
                    DivertSystems[i] = divertSystem;

                    GameObject taskHolder = new GameObject(elem.name);
                    taskHolder.transform.SetParent(_taskContainer.transform);

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
                taskHolder.transform.SetParent(_taskContainer.transform);

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
                    _wiresTask = task;
                }

                string? taskLengthProp = elem.properties.taskLength;
                TaskType taskLength = taskLengthProp != null ? _taskLengths[taskLengthProp] : taskData.TaskType;
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
            string[] keys = new string[_consoleIDIncrements.Keys.Count];
            _consoleIDIncrements.Keys.CopyTo(keys, 0);

            foreach (var key in keys)
            {
                byte count = (byte)_consoleIDIncrements[key];
                if (key == "task-breakers")
                    BreakerCount = count;
                if (key == "task-toilet")
                    ToiletCount = count;
                if (key == "task-towels")
                    TowelCount = count;
                if (key == "task-fuel2")
                    FuelCount = count;
                if (key == "task-waterwheel1")
                    WaterWheelCount = count;
                if (key == "task-align1")
                    AlignEngineCount = count;
                if (key == "task-records2")
                    RecordsCount = count;
                if (key == "task-wires")
                    WiresCount = count;
                _consoleIDIncrements[key] = 0;
            }

            // Wires Length
            if (_wiresTask != null)
            {
                _wiresTask.MaxStep = Math.Min(WiresCount, (byte)3);
            }
            _wiresTask = null;
        }
    }
}
