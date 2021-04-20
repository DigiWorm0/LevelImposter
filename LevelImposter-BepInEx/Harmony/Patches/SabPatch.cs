using HarmonyLib;
using LevelImposter.Map;
using LevelImposter.MinimapGen;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AddSystemTask))]
    public static class SabPatch
    {
        public static bool Prefix(SystemTypes ENHDELNCBNG)
        {
            // Get Task
            PlayerTask playerTask = null;
            for (int i = 0; i < ShipStatus.Instance.SpecialTasks.Length; i++)
            {
                PlayerTask task = ShipStatus.Instance.SpecialTasks[i];
                if (task.StartAt == ENHDELNCBNG)
                {
                    playerTask = task;
                    break;
                }
            }

            // Check
            if (playerTask == null)
            {
                LILogger.LogError("Player has been given invalid System Task: " + ENHDELNCBNG.ToString());
                return false;
            }

            // Provide
            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            PlayerTask playerTask2 = GameObject.Instantiate<PlayerTask>(playerTask, localPlayer.transform);
            playerTask2.Id = 255U;
            playerTask2.Owner = localPlayer;
            playerTask2.Initialize();
            localPlayer.myTasks.Add(playerTask2);
            return false;
        }
    }
}
