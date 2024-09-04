using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class TaskConsoleBuilder
{
    private static readonly Dictionary<string, int> CONSOLE_ID_PAIRS = new()
    {
        { "task-garbage2", 1 },
        { "task-garbage3", 0 },
        { "task-garbage4", 2 },
        { "task-fans1", 0 },
        { "task-fans2", 1 },
        { "task-records1", 0 },
        { "task-pistols1", 1 },
        { "task-pistols2", 1 },
        { "task-vegetables1", 0 },
        { "task-vegetables2", 2 },
        { "task-egg2", 0 },
        { "task-marshmallow2", 0 },
        { "task-samples2", 0 },
        { "task-fish1", 0 },
        { "task-fish2", 1 },
        { "task-replaceparts1", 0 }
    };

    private static readonly Dictionary<string, int> CONSOLE_ID_INCREMENTS = new()
    {
        { "task-toilet", 0 },
        { "task-breakers", 0 },
        { "task-towels", 0 },
        { "task-node", 0 },
        { "task-waterwheel1", 0 },
        { "task-fuel2", 0 },
        { "task-align1", 0 },
        { "task-records2", 1 },
        { "task-wires", 0 },
        { "task-marshmallow1", 1 },
        { "task-egg1", 1 },
        { "task-samples1", 1 },
        { "task-replaceparts2", 1 },
        { "task-hoist", 0 }
    };

    private int _consoleID;

    private readonly Dictionary<string, int> _consoleIDIncrements = new(CONSOLE_ID_INCREMENTS);

    public TaskConsoleBuilder()
    {
        TowelPickupCount = null;
    }

    public static byte BreakerCount { get; private set; }

    public static byte ToiletCount { get; private set; }

    public static byte TowelCount { get; private set; }

    public static byte FuelCount { get; private set; }

    public static byte WaterWheelCount { get; private set; }

    public static byte AlignEngineCount { get; private set; }

    public static byte RecordsCount { get; private set; }

    public static byte WiresCount { get; private set; }

    public static byte? TowelPickupCount { get; private set; }

    /// <summary>
    ///     Constructs a Console component for an LIElement, GameObject, and prefab
    /// </summary>
    /// <param name="elem">LIElement to construct</param>
    /// <param name="obj">GameObject to attatch component to</param>
    /// <param name="prefab">Prefab to copy defaults from</param>
    /// <returns>Cooresponding Console component</returns>
    public Console Build(LIElement elem, GameObject obj, GameObject prefab)
    {
        // Prefab
        var prefabConsole = prefab.GetComponentInChildren<Console>();

        // Specific Types
        var isArms = elem.type == "task-pistols1" || elem.type == "task-rifles1";
        var isTowel = elem.type.StartsWith("task-towels") && elem.type != "task-towels1";

        // Console
        Console console;
        if (isArms)
        {
            console = obj.AddComponent<StoreArmsTaskConsole>();
            var specialConsole = console.Cast<StoreArmsTaskConsole>();
            var origSpecialConsole = prefabConsole.Cast<StoreArmsTaskConsole>();

            specialConsole.timesUsed = origSpecialConsole.timesUsed;
            specialConsole.Images = origSpecialConsole.Images;
            specialConsole.useSound = origSpecialConsole.useSound;
            specialConsole.usesPerStep = origSpecialConsole.usesPerStep;
        }
        else if (isTowel)
        {
            console = obj.AddComponent<TowelTaskConsole>();
            var specialConsole = console.Cast<TowelTaskConsole>();
            var origSpecialConsole = prefabConsole.Cast<TowelTaskConsole>();

            specialConsole.useSound = origSpecialConsole.useSound;
            TowelPickupCount = elem.properties.towelPickupCount != null
                ? (byte)elem.properties.towelPickupCount
                : null;
        }
        else
        {
            console = obj.AddComponent<Console>();
        }

        console.Image = obj.GetComponent<SpriteRenderer>();
        console.ConsoleId = GetConsoleID(elem.type);
        console.Room = RoomBuilder.GetParentOrDefault(elem);
        console.TaskTypes = prefabConsole.TaskTypes;
        console.ValidTasks = GetConsoleTasks(elem.type, console.ConsoleId) ?? prefabConsole.ValidTasks;
        console.AllowImpostor = false;
        console.checkWalls = elem.properties.checkCollision ?? false;
        console.onlyFromBelow = elem.properties.onlyFromBelow ?? false;
        console.usableDistance = elem.properties.range ?? 1.0f;

        return console;
    }


    /// <summary>
    ///     Gets the ID to apply onto a task Console
    /// </summary>
    /// <param name="type">LIElement Type</param>
    /// <returns>int representing the Console ID to apply</returns>
    private int GetConsoleID(string type)
    {
        var isTowels = type.StartsWith("task-towels");

        if (CONSOLE_ID_PAIRS.ContainsKey(type))
            return CONSOLE_ID_PAIRS[type];
        if (isTowels)
            return type == "task-towels1" ? 255 : _consoleIDIncrements["task-towels"]++;
        if (_consoleIDIncrements.ContainsKey(type))
            return _consoleIDIncrements[type]++;
        return _consoleID++;
    }

    /// <summary>
    ///     Gets the TaskSet[] to apply onto a task Console
    /// </summary>
    /// <param name="type">LIElement Type</param>
    /// <param name="consoleID">ID of the Console object</param>
    /// <returns>A Il2Cpp Reference Array of TaskSets or NULL if default should be used</returns>
    private Il2CppReferenceArray<TaskSet>? GetConsoleTasks(string type, int consoleID)
    {
        var isWaterJug = type == "task-waterjug2";
        var isFuel = type == "task-fuel1";
        var isFuelOutput = type == "task-fuel2";
        var isHoist = type == "task-hoist";

        if (isWaterJug)
        {
            TaskSet taskSet = new()
            {
                taskType = TaskTypes.ReplaceWaterJug,
                taskStep = new IntRange(1, 1)
            };
            return new Il2CppReferenceArray<TaskSet>(new[] { taskSet });
        }

        if (isFuel)
        {
            var taskArr = new Il2CppReferenceArray<TaskSet>(byte.MaxValue / 2);
            for (byte i = 0; i < byte.MaxValue - 1; i += 2)
            {
                TaskSet taskSet = new()
                {
                    taskType = TaskTypes.FuelEngines,
                    taskStep = new IntRange(i, i)
                };
                taskArr[i / 2] = taskSet;
            }

            return taskArr;
        }

        if (isFuelOutput)
        {
            TaskSet taskSet = new()
            {
                taskType = TaskTypes.FuelEngines,
                taskStep = new IntRange(consoleID * 2 + 1, consoleID * 2 + 1)
            };
            return new Il2CppReferenceArray<TaskSet>(new[] { taskSet });
        }

        if (isHoist)
        {
            TaskSet taskSet = new()
            {
                taskType = TaskTypes.HoistSupplies,
                taskStep = new IntRange(consoleID, consoleID)
            };
            return new Il2CppReferenceArray<TaskSet>(new[] { taskSet });
        }

        return null;
    }

    /// <summary>
    ///     Performs final clean-up
    /// </summary>
    public void PostBuild()
    {
        var keys = new string[_consoleIDIncrements.Keys.Count];
        _consoleIDIncrements.Keys.CopyTo(keys, 0);

        foreach (var key in keys)
        {
            var count = (byte)_consoleIDIncrements[key];
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
    }
}