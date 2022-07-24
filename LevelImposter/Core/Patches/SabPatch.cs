using HarmonyLib;
using System;
using System.Collections.Generic;
using UnhollowerBaseLib.Attributes;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      By default, sabotages have hard-coded
     *      indices in ShipStatus. This patch will
     *      allow for more flexibility
     */
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AddSystemTask))]
    public static class SabPatch
    {
        private static Dictionary<SystemTypes, TaskTypes> systemTaskPairs = new Dictionary<SystemTypes, TaskTypes> {
            { SystemTypes.Electrical, TaskTypes.FixLights },
            { SystemTypes.Reactor, TaskTypes.ResetReactor },
            { SystemTypes.LifeSupp, TaskTypes.RestoreOxy },
            { SystemTypes.Comms, TaskTypes.FixComms },
        };

        public static bool Prefix([HarmonyArgument(0)] SystemTypes systemType)
        {
            if (MapLoader.currentMap == null)
                return true;

            TaskTypes taskType = systemTaskPairs[systemType];
            foreach (PlayerTask task in ShipStatus.Instance.SpecialTasks)
            {
                if (task.TaskType == taskType)
                {
                    PlayerControl localPlayer = PlayerControl.LocalPlayer;
                    PlayerTask taskClone = UnityEngine.Object.Instantiate<PlayerTask>(task, localPlayer.transform);
                    taskClone.Id = 255U;
                    taskClone.Owner = localPlayer;
                    taskClone.Initialize();
                    localPlayer.myTasks.Add(taskClone);
                    return false;
                }
            }

            return true;
        }
    }
}