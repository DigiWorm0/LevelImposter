using HarmonyLib;
using LevelImposter.Builders;
using LevelImposter.Trigger;
using System.Collections.Generic;

namespace LevelImposter.Core
{
    /// <summary>
    /// Normally, sabotages have hard-coded indices in ShipStatus.
    /// This patch will allow for more flexibility.
    /// 
    /// In addition, calls the sabotage start trigger.
    /// </summary>
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AddSystemTask))]
    public static class SabStartPatch
    {
        private static Dictionary<SystemTypes, TaskTypes> _systemTaskPairs = new()
        {
            { SystemTypes.Electrical, TaskTypes.FixLights },
            { SystemTypes.Reactor, TaskTypes.ResetReactor },
            { SystemTypes.LifeSupp, TaskTypes.RestoreOxy },
            { SystemTypes.Comms, TaskTypes.FixComms },
            { SystemTypes.MushroomMixupSabotage, TaskTypes.MushroomMixupSabotage }
        };
        private static Dictionary<SystemTypes, string> _systemTriggerPairs = new()
        {
            { SystemTypes.Electrical, "onLightsStart" },
            { SystemTypes.Reactor, "onReactorStart" },
            { SystemTypes.LifeSupp, "onOxygenStart" },
            { SystemTypes.Comms, "onCommsStart" },
            { SystemTypes.MushroomMixupSabotage, "onMixupStart" }
        };

        public static bool Prefix([HarmonyArgument(0)] SystemTypes systemType)
        {
            if (LIShipStatus.Instance == null)
                return true;

            // Get TaskType and TriggerName
            TaskTypes taskType = _systemTaskPairs[systemType];
            string triggerName = _systemTriggerPairs[systemType];

            // Search for Sabotage
            foreach (PlayerTask task in ShipStatus.Instance.SpecialTasks)
            {
                if (task.TaskType == taskType)
                {
                    // Create Task
                    PlayerControl localPlayer = PlayerControl.LocalPlayer;
                    PlayerTask taskClone = UnityEngine.Object.Instantiate(task, localPlayer.transform);
                    taskClone.Id = 255U;
                    taskClone.Owner = localPlayer;
                    taskClone.Initialize();
                    localPlayer.myTasks.Add(taskClone);

                    // Fire Trigger
                    if (SabotageOptionsBuilder.TriggerObject != null)
                        TriggerSystem.Trigger(SabotageOptionsBuilder.TriggerObject, triggerName, null);
                    return false;
                }
            }

            // Sabotage Not Found
            LILogger.Warn($"Could not find sabotage for {systemType}");
            return true;
        }
    }

}