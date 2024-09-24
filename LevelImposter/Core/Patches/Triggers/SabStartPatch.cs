using System.Collections.Generic;
using HarmonyLib;
using LevelImposter.Builders;
using LevelImposter.Trigger;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Normally, sabotages have hard-coded indices in ShipStatus.
///     This patch will allow for more flexibility.
///     In addition, calls the sabotage start trigger.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AddSystemTask))]
public static class SabStartPatch
{
    private static readonly Dictionary<SystemTypes, TaskTypes> _systemTaskPairs = new()
    {
        { SystemTypes.Electrical, TaskTypes.FixLights },
        { SystemTypes.Reactor, TaskTypes.ResetReactor },
        { SystemTypes.LifeSupp, TaskTypes.RestoreOxy },
        { SystemTypes.Comms, TaskTypes.FixComms },
        { SystemTypes.MushroomMixupSabotage, TaskTypes.MushroomMixupSabotage }
    };

    private static readonly Dictionary<SystemTypes, string> _systemTriggerPairs = new()
    {
        { SystemTypes.Electrical, "onLightsStart" },
        { SystemTypes.Reactor, "onReactorStart" },
        { SystemTypes.LifeSupp, "onOxygenStart" },
        { SystemTypes.Comms, "onCommsStart" },
        { SystemTypes.MushroomMixupSabotage, "onMixupStart" }
    };

    public static bool Prefix([HarmonyArgument(0)] SystemTypes systemType)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        // Get TaskType and TriggerName
        var taskType = _systemTaskPairs[systemType];
        var triggerName = _systemTriggerPairs[systemType];

        // Search for Sabotage
        foreach (var task in ShipStatus.Instance.SpecialTasks)
            if (task.TaskType == taskType)
            {
                // Create Task
                var localPlayer = PlayerControl.LocalPlayer;
                var taskClone = Object.Instantiate(task, localPlayer.transform);
                taskClone.Id = 255U;
                taskClone.Owner = localPlayer;
                taskClone.Initialize();
                localPlayer.myTasks.Add(taskClone);

                // Fire Trigger
                if (SabotageOptionsBuilder.TriggerObject != null)
                {
                    TriggerSignal signal = new(SabotageOptionsBuilder.TriggerObject, triggerName,
                        PlayerControl.LocalPlayer);
                    TriggerSystem.GetInstance().FireTrigger(signal);
                }

                return false;
            }

        // Sabotage Not Found
        LILogger.Warn($"Could not find sabotage for {systemType}");
        return true;
    }
}