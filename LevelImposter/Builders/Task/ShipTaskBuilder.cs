using System;
using System.Collections.Generic;
using System.Linq;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.Shop;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders;

public class ShipTaskBuilder : IElemBuilder
{
    private static readonly Dictionary<string, TaskLength> TaskLengths = new()
    {
        { "Short", TaskLength.Short },
        { "Long", TaskLength.Long },
        { "Common", TaskLength.Common }
    };

    private readonly List<string> _builtTypes = [];
    private GameObject? _taskParent;
    private NormalPlayerTask? _wiresTask;

    public static SystemTypes[] DivertSystems { get; private set; } = Array.Empty<SystemTypes>();
    
    public void OnPreBuild()
    {
        _builtTypes.Clear();
        _taskParent = null;
        _wiresTask = null;
        DivertSystems = Array.Empty<SystemTypes>();
    }

    /// <summary>
    ///     Builds a NormalPlayerTask from a LIElement
    ///     and Console then adds it to ShipStatus
    /// </summary>
    /// <param name="elem">LIElement representing the task</param>
    /// <param name="console">Console the task is from</param>
    /// <exception cref="Exception"></exception>
    public void Build(LIElement elem, Console console)
    {
        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;
        if (shipStatus == null)
            throw new MissingShipException();

        // Task Container
        if (_taskParent == null)
        {
            _taskParent = new GameObject("Tasks");
            _taskParent.transform.SetParent(shipStatus.transform);
        }

        // Values
        var hasTask = AssetDB.HasTask(elem.type);
        var isDivert = elem.type == "task-divert1";
        var isNode = elem.type == "task-node";
        var isNodeSwitch = elem.type == "task-nodeswitch";
        var isWires = elem.type == "task-wires";
        var isDownload = elem.type == "task-download";

        // Prefab
        var prefabTask = hasTask ? AssetDB.GetTask<NormalPlayerTask>(elem.type) : null;
        var prefabArrow = prefabTask?.Arrow?.gameObject;
        var prefabLength = hasTask ? AssetDB.GetTaskLength(elem.type) : TaskLength.Common;
        var systemType = RoomBuilder.GetParentOrDefault(elem);

        // Rename
        var renameHandler = LIBaseShip.Instance?.Renames;
        if (prefabTask != null && !string.IsNullOrEmpty(elem.properties.description))
        {
            renameHandler?.Add(prefabTask.TaskType, elem.properties.description);

            // Rename Node Description
            if (isNode || isNodeSwitch)
                renameHandler?.Add(StringNames.FixWeatherNode, elem.properties.description);
        }

        // Rename Node Room
        if (isNode)
        {
            var controlType = WeatherSwitchGame.ControlNames[console.ConsoleId];
            var roomName = renameHandler?.Get(systemType);
            if (roomName != null)
                renameHandler?.Add(controlType, roomName);
        }

        // Built List
        var isBuilt = _builtTypes.Contains(elem.type);
        if (!isBuilt)
        {
            LILogger.Debug($" + Creating task for {elem}...");
            _builtTypes.Add(elem.type);
        }

        // TODO: Clean this spaghetti mess
        if (isDivert && !isBuilt)
        {
            if (prefabTask == null)
                throw new Exception("Divert task prefab is null");

            var divertTargets = FindElementsOfType("task-divert2");

            DivertSystems = new SystemTypes[divertTargets.Count];
            for (var i = 0; i < divertTargets.Count; i++)
            {
                var divertTarget = divertTargets[i];

                var divertSystem = RoomBuilder.GetParentOrDefault(divertTarget);
                DivertSystems[i] = divertSystem;

                GameObject taskContainer = new(elem.name);
                taskContainer.transform.SetParent(_taskParent.transform);

                var task = taskContainer.AddComponent<DivertPowerTask>();
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
                    var arrow = Object.Instantiate(prefabArrow, taskContainer.transform, true);
                    arrow.SetActive(false);
                    task.Arrow = arrow.GetComponent<ArrowBehaviour>();
                }

                AddTaskToShip(elem, prefabLength, task);
            }
        }
        else if (hasTask && (!isBuilt || isNode))
        {
            if (prefabTask == null)
                throw new Exception("Task prefab is null");

            GameObject taskContainer = new(elem.name);
            taskContainer.transform.SetParent(_taskParent.transform);

            var task = taskContainer.AddComponent(prefabTask.GetIl2CppType()).Cast<NormalPlayerTask>();
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

            task.useMultipleText = prefabTask.useMultipleText;
            task.maxNumStepsStage1 = prefabTask.maxNumStepsStage1;
            task.textStage1 = prefabTask.textStage1;
            task.textStage2 = prefabTask.textStage2;

            if (prefabArrow != null)
            {
                var arrow = Object.Instantiate(prefabArrow, taskContainer.transform, true);
                arrow.SetActive(false);
                task.Arrow = arrow.GetComponent<ArrowBehaviour>();
            }

            if (isNode)
            {
                var nodeTask = task.Cast<WeatherNodeTask>();
                nodeTask.NodeId = console.ConsoleId;
                nodeTask.Stage2Prefab = prefabTask.Cast<WeatherNodeTask>().Stage2Prefab;
            }

            if (isDownload)
            {
                var downloadTask = task.Cast<UploadDataTask>();
                var uploadTargets = FindElementsOfType("task-upload");
                if (uploadTargets.Count > 0)
                    downloadTask.EndAt = RoomBuilder.GetParentOrDefault(uploadTargets[0]);
            }

            if (isWires)
                _wiresTask = task;

            AddTaskToShip(elem, prefabLength, task);
        }
    }
    
    /// <summary>
    ///     Performs final clean-up
    /// </summary>
    public void OnPostBuild()
    {
        if (_wiresTask != null)
            _wiresTask.MaxStep = Math.Min(TaskConsoleBuilder.WiresCount, (byte)3);
    }

    /// <summary>
    ///     Finds a list of elements of the specified type
    /// </summary>
    /// <param name="type">Type to search for</param>
    /// <returns>List of all elements in the map of the cooresponding type</returns>
    /// <exception cref="Exception">If there is no LIMap loaded/loading</exception>
    private static List<LIElement> FindElementsOfType(string type)
    {
        // Check Map
        var currentMap = GameConfiguration.CurrentMap;
        if (currentMap == null)
            throw new Exception("Current map is unavailable");

        // Find Elements
        return currentMap.elements.Where(mapElem => mapElem.type == type).ToList();
    }

    /// <summary>
    ///     Adds a NormalPlayerTask to ShipStatus
    /// </summary>
    /// <param name="elem">Cooresponding task element</param>
    /// <param name="prefabLength">Default length of the task</param>
    /// <param name="task">Task to add</param>
    private static void AddTaskToShip(
        LIElement elem,
        TaskLength prefabLength,
        NormalPlayerTask task)
    {
        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;
        if (shipStatus == null)
            throw new MissingShipException();

        // TaskLength
        var taskLengthProp = elem.properties.taskLength;
        var taskLength = taskLengthProp != null ? TaskLengths[taskLengthProp] : prefabLength;
        switch (taskLength)
        {
            case TaskLength.Common:
                shipStatus.CommonTasks = MapUtils.AddToArr(shipStatus.CommonTasks, task);
                break;
            case TaskLength.Short:
                shipStatus.ShortTasks = MapUtils.AddToArr(shipStatus.ShortTasks, task);
                break;
            case TaskLength.Long:
                shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown task length: {taskLength}");
        }
    }
}