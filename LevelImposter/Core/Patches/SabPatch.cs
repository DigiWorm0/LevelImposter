using HarmonyLib;
using System.Collections.Generic;
using LevelImposter.Builders;

namespace LevelImposter.Core
{
    /*
     *      By default, sabotages have hard-coded
     *      indices in ShipStatus. This patch will
     *      allow for more flexibility
     */
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AddSystemTask))]
    public static class SabStartPatch
    {
        private static Dictionary<SystemTypes, TaskTypes> _systemTaskPairs = new()
        {
            { SystemTypes.Electrical, TaskTypes.FixLights },
            { SystemTypes.Reactor, TaskTypes.ResetReactor },
            { SystemTypes.LifeSupp, TaskTypes.RestoreOxy },
            { SystemTypes.Comms, TaskTypes.FixComms },
        };
        private static Dictionary<SystemTypes, string> _systemTriggerPairs = new()
        {
            { SystemTypes.Electrical, "onLightsStart" },
            { SystemTypes.Reactor, "onReactorStart" },
            { SystemTypes.LifeSupp, "onOxygenStart" },
            { SystemTypes.Comms, "onCommsStart" }
        };

        public static bool Prefix([HarmonyArgument(0)] SystemTypes systemType)
        {
            if (LIShipStatus.Instance == null)
                return true;

            TaskTypes taskType = _systemTaskPairs[systemType];
            string triggerName = _systemTriggerPairs[systemType];

            // Search for Sabotage
            foreach (PlayerTask task in ShipStatus.Instance.SpecialTasks)
            {
                if (task.TaskType == taskType)
                {
                    PlayerControl localPlayer = PlayerControl.LocalPlayer;
                    PlayerTask taskClone = UnityEngine.Object.Instantiate(task, localPlayer.transform);
                    taskClone.Id = 255U;
                    taskClone.Owner = localPlayer;
                    taskClone.Initialize();
                    localPlayer.myTasks.Add(taskClone);

                    // Fire Trigger
                    LITriggerable.Trigger(SabotageOptionsBuilder.TriggerObject, triggerName, null);
                    return false;
                }
            }

            // Sabotage Not Found
            LILogger.Warn($"Could not find sabotage for {systemType}");
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveTask))]
    public static class SabEndPatch
    {
        private static Dictionary<TaskTypes, string> _taskTriggerPairs = new()
        {
            { TaskTypes.FixLights, "onLightsEnd" },
            { TaskTypes.ResetReactor, "onReactorEnd" },
            { TaskTypes.RestoreOxy, "onOxygenEnd" },
            { TaskTypes.FixComms, "onCommsEnd" }
        };

        public static void Postfix([HarmonyArgument(0)] PlayerTask task)
        {
            if (LIShipStatus.Instance == null)
                return;
            if (!_taskTriggerPairs.ContainsKey(task.TaskType))
                return;
            
            // Fire Trigger
            string triggerName = _taskTriggerPairs[task.TaskType];
            LITriggerable.Trigger(SabotageOptionsBuilder.TriggerObject, triggerName, null);
        }
    }
}