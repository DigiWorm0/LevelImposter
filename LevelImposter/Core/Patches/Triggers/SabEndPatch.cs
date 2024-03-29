﻿using HarmonyLib;
using LevelImposter.Builders;
using System.Collections.Generic;

namespace LevelImposter.Core
{
    /// <summary>
    /// Calls the trigger when the sabotage is finished.
    /// </summary>
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveTask))]
    public static class SabEndPatch
    {
        private static Dictionary<TaskTypes, string> _taskTriggerPairs = new()
        {
            { TaskTypes.FixLights, "onLightsEnd" },
            { TaskTypes.ResetReactor, "onReactorEnd" },
            { TaskTypes.RestoreOxy, "onOxygenEnd" },
            { TaskTypes.FixComms, "onCommsEnd" },
            { TaskTypes.MushroomMixupSabotage, "onMixupEnd" }
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
