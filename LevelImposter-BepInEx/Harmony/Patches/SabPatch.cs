using HarmonyLib;
using LevelImposter.Map;
using LevelImposter.MinimapGen;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(ReactorTask), nameof(ReactorTask.Awake))]
    public static class ReactorPatch
    {
        public static void Prefix(ReactorTask __instance)
        {
            __instance.StartAt = SystemTypes.Laboratory;
        }
    }

    [HarmonyPatch(typeof(ReactorTask), nameof(ReactorTask.Complete))]
    public static class SabArrowFix1
    {
        public static void Postfix()
        {
            GameObject.Find("SabManager").transform.GetChild(0).gameObject.SetActive(false);
            GameObject.Find("SabManager").transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    [HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.Complete))]
    public static class SabArrowFix2
    {
        public static void Postfix()
        {
            GameObject.Find("SabManager").transform.GetChild(0).gameObject.SetActive(false);
            GameObject.Find("SabManager").transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    [HarmonyPatch(typeof(HudOverrideTask), nameof(HudOverrideTask.Complete))]
    public static class SabArrowFix3
    {
        public static void Postfix()
        {
            GameObject.Find("SabManager").transform.GetChild(0).gameObject.SetActive(false);
            GameObject.Find("SabManager").transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AddSystemTask))]
    public static class SabPatch
    {
        public static bool Prefix([HarmonyArgument(0)] SystemTypes systemTask)
        {
            // Get Task
            PlayerTask playerTask = null;
            for (int i = 0; i < ShipStatus.Instance.SpecialTasks.Length; i++)
            {
                PlayerTask task = ShipStatus.Instance.SpecialTasks[i];
                if (task.StartAt == systemTask)
                {
                    playerTask = task;
                    break;
                }
            }

            // Check
            if (playerTask == null)
            {
                LILogger.LogError("Player has been given invalid System Task: " + systemTask.ToString());
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
