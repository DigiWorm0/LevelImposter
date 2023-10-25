using LevelImposter.Core;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class ShipTaskBuilder
    {
        private static readonly Dictionary<string, TaskLength> TASK_LENGTHS = new()
        {
            { "Short", TaskLength.Short },
            { "Long", TaskLength.Long },
            { "Common", TaskLength.Common }
        };

        private readonly List<string> WHITELIST_TYPES = new()
        {
            "task-node"
        };

        private List<string> _builtTypes = new();
        private GameObject _taskParent = null;
        private NormalPlayerTask? _wiresTask = null;

        private static SystemTypes[] _divertSystems = Array.Empty<SystemTypes>();

        public static SystemTypes[] DivertSystems => _divertSystems;

        /// <summary>
        /// Builds a NormalPlayerTask from a LIElement
        /// and Console then adds it to ShipStatus
        /// </summary>
        /// <param name="elem">LIElement representing the task</param>
        /// <param name="console">Console the task is from</param>
        /// <exception cref="Exception"></exception>
        public void Build(LIElement elem, Console console)
        {
            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Task Container
            if (_taskParent == null)
            {
                _taskParent = new GameObject("Tasks");
                _taskParent.transform.SetParent(shipStatus.transform);
            }

            // Values
            bool hasTask = AssetDB.HasTask(elem.type);
            bool isDivert = elem.type == "task-divert1";
            bool isNode = elem.type == "task-node";
            bool isWires = elem.type == "task-wires";

            // Prefab
            var prefabTask = hasTask ? AssetDB.GetTask<NormalPlayerTask>(elem.type) : null;
            var prefabArrow = prefabTask?.Arrow?.gameObject;
            var prefabLength = hasTask ? AssetDB.GetTaskLength(elem.type) : TaskLength.Common;
            SystemTypes systemType = RoomBuilder.GetParentOrDefault(elem);

            // Rename
            if (prefabTask != null && !string.IsNullOrEmpty(elem.properties.description))
                LIShipStatus.Instance?.Renames.Add(prefabTask.TaskType, elem.properties.description);

            // Built List
            bool isBuilt = _builtTypes.Contains(elem.type);
            if (!isBuilt)
            {
                LILogger.Info($" + Creating task for {elem}...");
                _builtTypes.Add(elem.type);
            }

            // TODO: Clean this spaghetti mess
            if (isDivert && !isBuilt)
            {
                List<LIElement> divertTargets = new();
                if (LIShipStatus.Instance?.CurrentMap == null)
                    throw new Exception("Current map is unavailable");
                foreach (LIElement mapElem in LIShipStatus.Instance.CurrentMap.elements)
                    if (mapElem.type == "task-divert2")
                        divertTargets.Add(mapElem);

                _divertSystems = new SystemTypes[divertTargets.Count];
                for (int i = 0; i < divertTargets.Count; i++)
                {
                    LIElement divertTarget = divertTargets[i];

                    SystemTypes divertSystem = RoomBuilder.GetParentOrDefault(divertTarget);
                    _divertSystems[i] = divertSystem;

                    GameObject taskContainer = new(elem.name);
                    taskContainer.transform.SetParent(_taskParent.transform);

                    DivertPowerTask task = taskContainer.AddComponent<DivertPowerTask>();
                    task.StartAt = systemType;
                    task.taskStep = prefabTask.taskStep;
                    task.MaxStep = prefabTask.MaxStep;
                    task.arrowSuspended = prefabTask.arrowSuspended;
                    task.ShowTaskTimer = prefabTask.ShowTaskTimer;
                    task.ShowTaskStep = prefabTask.ShowTaskStep;
                    task.TaskTimer = prefabTask.TaskTimer;
                    task.TimerStarted = prefabTask.TimerStarted;
                    task.TaskType = prefabTask.TaskType;
                    task.MinigamePrefab = prefabTask.MinigamePrefab;
                    task.TargetSystem = divertSystem;

                    if (prefabArrow != null)
                    {
                        GameObject arrow = UnityEngine.Object.Instantiate(prefabArrow);
                        arrow.transform.SetParent(taskContainer.transform);
                        arrow.SetActive(false);
                        task.Arrow = arrow.GetComponent<ArrowBehaviour>();
                    }

                    AddTaskToShip(elem, prefabLength, task);
                }
            }
            else if (hasTask && (!isBuilt || isNode))
            {
                GameObject taskContainer = new(elem.name);
                taskContainer.transform.SetParent(_taskParent.transform);

                NormalPlayerTask task = taskContainer.AddComponent(prefabTask.GetIl2CppType()).Cast<NormalPlayerTask>();
                task.StartAt = systemType;
                task.taskStep = prefabTask.taskStep;
                task.MaxStep = prefabTask.MaxStep;
                task.arrowSuspended = prefabTask.arrowSuspended;
                task.ShowTaskTimer = prefabTask.ShowTaskTimer;
                task.ShowTaskStep = prefabTask.ShowTaskStep;
                task.TaskTimer = prefabTask.TaskTimer;
                task.TimerStarted = prefabTask.TimerStarted;
                task.TaskType = prefabTask.TaskType;
                task.MinigamePrefab = prefabTask.MinigamePrefab;

                if (prefabArrow != null)
                {
                    GameObject arrow = UnityEngine.Object.Instantiate(prefabArrow);
                    arrow.transform.SetParent(taskContainer.transform);
                    arrow.SetActive(false);
                    task.Arrow = arrow.GetComponent<ArrowBehaviour>();
                }

                if (isNode)
                {
                    WeatherNodeTask nodeTask = task.Cast<WeatherNodeTask>();
                    nodeTask.NodeId = console.ConsoleId;
                    nodeTask.Stage2Prefab = prefabTask.Cast<WeatherNodeTask>().Stage2Prefab;
                }

                if (isWires)
                    _wiresTask = task;

                AddTaskToShip(elem, prefabLength, task);
            }
        }

        /// <summary>
        /// Performs final clean-up
        /// </summary>
        public void PostBuild()
        {
            if (_wiresTask != null)
                _wiresTask.MaxStep = Math.Min(TaskConsoleBuilder.WiresCount, (byte)3);
            _wiresTask = null;
        }

        /// <summary>
        /// Adds a NormalPlayerTask to ShipStatus
        /// </summary>
        /// <param name="elem">Cooresponding task element</param>
        /// <param name="prefabLength">Default length of the task</param>
        /// <param name="task">Task to add</param>
        private void AddTaskToShip(
            LIElement elem,
            TaskLength prefabLength,
            NormalPlayerTask task)
        {
            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // TaskLength
            string? taskLengthProp = elem.properties.taskLength;
            TaskLength taskLength = taskLengthProp != null ? TASK_LENGTHS[taskLengthProp] : prefabLength;
            if (taskLength == TaskLength.Common)
                shipStatus.CommonTasks = MapUtils.AddToArr(shipStatus.CommonTasks, task);
            if (taskLength == TaskLength.Short)
                shipStatus.ShortTasks = MapUtils.AddToArr(shipStatus.ShortTasks, task);
            if (taskLength == TaskLength.Long)
                shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task);
        }
    }
}
