using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using CollectionExtensions = HarmonyLib.CollectionExtensions;

namespace LevelImposter.Builders;

public class SabBuilder : IElemBuilder
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

    private GameObject? _sabContainer;

    public void OnPreBuild()
    {
        SabDB.Clear();
        _sabContainer = null;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("sab-") ||
            elem.type.StartsWith("sab-btn") ||
            elem.type.StartsWith("sab-door"))
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // Container
        if (_sabContainer == null)
        {
            _sabContainer = new GameObject("Sabotages");
            _sabContainer.transform.SetParent(shipStatus.transform);
            _sabContainer.SetActive(false);
        }

        // Prefab
        var prefabTask = AssetDB.GetTask<SabotageTask>(elem.type);
        if (prefabTask == null)
            return;

        // System
        var roomSystem = RoomBuilder.GetParentOrDefault(elem);

        // Task
        if (!SabDB.ContainsKey(roomSystem))
        {
            // Sabotage Task
            LILogger.Debug($" + Adding sabotage for {elem}...");
            var sabContainer = new GameObject(elem.name);
            sabContainer.transform.SetParent(_sabContainer.transform);

            // Create Task
            var task = sabContainer.AddComponent(prefabTask.GetIl2CppType()).Cast<SabotageTask>();
            task.StartAt = prefabTask.StartAt;
            task.TaskType = prefabTask.TaskType;
            task.MinigamePrefab = prefabTask.MinigamePrefab;
            task.Arrows = new Il2CppReferenceArray<ArrowBehaviour>(0);

            // Rename Task
            if (!string.IsNullOrEmpty(elem.properties.description))
                LIBaseShip.Instance?.Renames.Add(task.TaskType, elem.properties.description);

            // Add To Quick Chat
            var taskName = TranslationController.Instance.GetTaskName(task.TaskType);
            shipStatus.SystemNames = CollectionExtensions.AddItem(shipStatus.SystemNames, taskName).ToArray();

            // Add Task
            shipStatus.SpecialTasks = MapUtils.AddToArr(shipStatus.SpecialTasks, task);
            SabDB.Add(roomSystem, task);

            // Sabotage System
            var sabDuration = elem.properties.sabDuration;
            if (sabDuration == null)
                return;

            var hasSabSystem = SabSystems.TryGetValue(elem.type, out var sabSystemType);
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