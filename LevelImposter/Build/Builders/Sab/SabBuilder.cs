using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Build.Attributes;
using LevelImposter.Build.Builders.Util;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;
using CollectionExtensions = HarmonyLib.CollectionExtensions;

namespace LevelImposter.Build.Builders.Sab;

internal static class SabBuilder
{
    private static readonly Dictionary<SystemTypes, SabotageTask> SabDB = new();

    private static readonly Dictionary<string, SystemTypes> SabSystems = new()
    {
        { "sab-reactorleft", SystemTypes.Reactor },
        { "sab-reactorright", SystemTypes.Reactor },
        { "sab-btnreactor", SystemTypes.Reactor },
        { "sab-oxygen1", SystemTypes.LifeSupp },
        { "sab-oxygen2", SystemTypes.LifeSupp },
        { "sab-btnoxygen", SystemTypes.LifeSupp }
    };

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        SabDB.Clear();
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes =
        [
            "sab-reactorleft",
            "sab-reactorright",
            "sab-oxygen1",
            "sab-oxygen2",
            "sab-electric",
            "sab-comms"
        ])
    ]
    public static void Build(LIBaseShip baseShip, ShipStatus shipStatus, LIElement element)
    {
        // Prefab
        var prefabTask = PrefabDB.GetTask<SabotageTask>(element.type);
        if (prefabTask == null)
            return;

        // System
        var roomSystem = RoomBuilder.GetParentOrDefault(element);

        // Task
        if (!SabDB.ContainsKey(roomSystem))
        {
            // Sabotage Task
            LILogger.Debug($" + Adding sabotage for {element}...");
            var sabContainer = new GameObject(element.name);
            sabContainer.transform.SetParent(baseShip.Prefabs.Container);

            // Create Task
            var task = sabContainer.AddComponent(prefabTask.GetIl2CppType()).Cast<SabotageTask>();
            task.StartAt = prefabTask.StartAt;
            task.TaskType = prefabTask.TaskType;
            task.MinigamePrefab = prefabTask.MinigamePrefab;
            task.Arrows = new Il2CppReferenceArray<ArrowBehaviour>(0);

            // Rename Task
            if (!string.IsNullOrEmpty(element.properties.description))
                LIBaseShip.Instance?.Renames.Add(task.TaskType, element.properties.description);

            // Add To Quick Chat
            var taskName = TranslationController.Instance.GetTaskName(task.TaskType);
            shipStatus.SystemNames = CollectionExtensions.AddItem(shipStatus.SystemNames, taskName).ToArray();

            // Add Task
            shipStatus.SpecialTasks = shipStatus.SpecialTasks.Add(task);
            SabDB.Add(roomSystem, task);

            // Sabotage System
            var sabDuration = element.properties.sabDuration;
            if (sabDuration == null)
                return;

            var hasSabSystem = SabSystems.TryGetValue(element.type, out var sabSystemType);
            if (!hasSabSystem)
                return;

            // Remove Old System
            var oldSystem = shipStatus.Systems[sabSystemType].Cast<IActivatable>();
            var sabSystem = shipStatus.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
            sabSystem.specials.Remove(oldSystem);

            shipStatus.Systems[sabSystemType] = sabSystemType switch
            {
                // Add New System
                SystemTypes.Reactor => new ReactorSystemType((float)sabDuration, sabSystemType).Cast<ISystemType>(),
                SystemTypes.LifeSupp => new LifeSuppSystemType((float)sabDuration).Cast<ISystemType>(),
                _ => shipStatus.Systems[sabSystemType]
            };
            sabSystem.specials.Add(shipStatus.Systems[sabSystemType].Cast<IActivatable>());
        }
    }

    /// <summary>
    ///     Gets a SabotageTask from a SystemTypes
    /// </summary>
    /// <param name="systemType">SystemTypes to search for</param>
    /// <param name="sabotageTask">Output sabotage task</param>
    /// <returns>TRUE if found</returns>
    public static bool TryGetSabotage(SystemTypes systemType, out SabotageTask? sabotageTask)
    {
        return SabDB.TryGetValue(systemType, out sabotageTask);
    }
}