using LevelImposter.Core;
using LevelImposter.DB;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class SabBuilder : IElemBuilder
    {
        private static readonly Dictionary<string, SystemTypes> SAB_SYSTEMS = new()
        {
            { "sab-reactorleft", SystemTypes.Reactor },
            { "sab-reactorright", SystemTypes.Reactor },
            { "sab-btnreactor", SystemTypes.Reactor },
            { "sab-oxygen1", SystemTypes.LifeSupp },
            { "sab-oxygen2", SystemTypes.LifeSupp },
            { "sab-btnoxygen", SystemTypes.LifeSupp },
        };

        private static Dictionary<SystemTypes, SabotageTask> _sabDB = new();
        private GameObject? _sabContainer = null;

        public SabBuilder()
        {
            _sabDB.Clear();
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("sab-") ||
                elem.type.StartsWith("sab-btn") ||
                elem.type.StartsWith("sab-door"))
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

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
            SystemTypes roomSystem = RoomBuilder.GetParentOrDefault(elem);

            // Task
            if (!_sabDB.ContainsKey(roomSystem))
            {
                // Sabotage Task
                LILogger.Info($" + Adding sabotage for {elem}...");
                GameObject sabContainer = new GameObject(elem.name);
                sabContainer.transform.SetParent(_sabContainer.transform);

                // Create Task
                SabotageTask task = sabContainer.AddComponent(prefabTask.GetIl2CppType()).Cast<SabotageTask>();
                task.StartAt = prefabTask.StartAt;
                task.TaskType = prefabTask.TaskType;
                task.MinigamePrefab = prefabTask.MinigamePrefab;
                task.Arrows = new(0);

                // Rename Task
                if (!string.IsNullOrEmpty(elem.properties.description))
                    LIShipStatus.Instance?.Renames.Add(task.TaskType, elem.properties.description);

                // Add To Quick Chat
                StringNames taskName = TranslationController.Instance.GetTaskName(task.TaskType);
                shipStatus.SystemNames = HarmonyLib.CollectionExtensions.AddItem(shipStatus.SystemNames, taskName).ToArray();

                // Add Task
                shipStatus.SpecialTasks = MapUtils.AddToArr(shipStatus.SpecialTasks, task);
                _sabDB.Add(roomSystem, task);

                // Sabotage System
                float? sabDuration = elem.properties.sabDuration;
                if (sabDuration == null)
                    return;

                bool hasSabSystem = SAB_SYSTEMS.TryGetValue(elem.type, out SystemTypes sabSystemType);
                if (!hasSabSystem)
                    return;

                // Remove Old System
                var oldSystem = shipStatus.Systems[sabSystemType].Cast<IActivatable>();
                var sabSystem = shipStatus.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
                sabSystem.specials.Remove(oldSystem);

                // Add New System
                if (sabSystemType == SystemTypes.Reactor)
                    shipStatus.Systems[sabSystemType] = new ReactorSystemType((float)sabDuration, sabSystemType).Cast<ISystemType>();
                if (sabSystemType == SystemTypes.LifeSupp)
                    shipStatus.Systems[sabSystemType] = new LifeSuppSystemType((float)sabDuration).Cast<ISystemType>();
                sabSystem.specials.Add(shipStatus.Systems[sabSystemType].Cast<IActivatable>());
            }
        }

        public void PostBuild() { }

        /// <summary>
        /// Gets a SabotageTask from a SystemTypes
        /// </summary>
        /// <param name="systemType">SystemTypes to search for</param>
        /// <param name="sabotageTask">Output sabotage task</param>
        /// <returns>TRUE if found</returns>
        public static bool TryGetSabotage(SystemTypes systemType, out SabotageTask? sabotageTask)
        {
            return _sabDB.TryGetValue(systemType, out sabotageTask);
        }
    }
}
