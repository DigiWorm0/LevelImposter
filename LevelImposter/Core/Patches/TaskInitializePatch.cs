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
     *      By default, various tasks have hard-coded
     *      counts and limits. In order to allow for
     *      flexibility, this has been patched here.
     */
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
    public static class TaskInitializePatch
    {
        public static void Postfix(NormalPlayerTask __instance)
        {
            if (MapLoader.currentMap == null)
                return;

            TaskTypes taskType = __instance.TaskType;
            switch (taskType)
            {
                case TaskTypes.ResetBreakers:
                    __instance.Data = new byte[TaskBuilder.breakerCount];
                    for (byte i = 0; i < TaskBuilder.breakerCount; i++)
                        __instance.Data[i] = i;
                    __instance.Data = MapUtils.Shuffle(__instance.Data);
                    __instance.MaxStep = TaskBuilder.breakerCount;
                    break;
                case TaskTypes.CleanToilet:
                    __instance.Data = new byte[1];
                    __instance.Data[0] = IntRange.NextByte(0, TaskBuilder.toiletCount);
                    break;
                case TaskTypes.PickUpTowels:
                    __instance.Data = new byte[TaskBuilder.towelCount / 2];
                    byte[] tempData = new byte[TaskBuilder.towelCount];
                    for (byte i = 0; i < TaskBuilder.towelCount; i++)
                        tempData[i] = i;
                    tempData = MapUtils.Shuffle(tempData);
                    for (byte i = 0; i < __instance.Data.Count; i++)
                        __instance.Data[i] = tempData[i];
                    break;
                case TaskTypes.FuelEngines:
                    __instance.MaxStep = TaskBuilder.fuelCount;
                    break;
                case TaskTypes.AlignEngineOutput:
                    __instance.Data = new byte[TaskBuilder.alignEngineCount + 2];
                    for (int i = 0; i < TaskBuilder.alignEngineCount; i++)
                        __instance.Data[i] = (byte)(IntRange.RandomSign() * IntRange.Next(25, 127) + 127);
                    __instance.MaxStep = TaskBuilder.alignEngineCount;
                    break;
            }
        }
    }

    /*
     *      Of course, WaterWayTask has to
     *      override NormalPlayerTask for no reason...
     */
    [HarmonyPatch(typeof(WaterWayTask), nameof(WaterWayTask.Initialize))]
    public static class WaterWheelInitializePatch
    {
        public static void Postfix(NormalPlayerTask __instance)
        {
            if (MapLoader.currentMap == null)
                return;

            __instance.Data = new byte[TaskBuilder.waterWheelCount];
            __instance.MaxStep = TaskBuilder.waterWheelCount;
        }
    }

    /*
     *      Fix Divert Power Sliders
     */
    [HarmonyPatch(typeof(DivertPowerMinigame), nameof(DivertPowerMinigame.Begin))]
    public static class DivertBeginPatch
    {
        public static void Prefix([HarmonyArgument(0)] PlayerTask task, DivertPowerMinigame __instance)
        {
            if (MapLoader.currentMap == null)
                return;

            __instance.SliderOrder = TaskBuilder.divertSystems;
        }
    }

    /*
     *      Fix Fuel Stages
     */
    [HarmonyPatch(typeof(MultistageMinigame), nameof(MultistageMinigame.Begin))]
    public static class MinigameBeginPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerTask task, MultistageMinigame __instance)
        {
            if (MapLoader.currentMap == null)
                return true;

            NormalPlayerTask normalPlayerTask = task.Cast<NormalPlayerTask>();
            if (normalPlayerTask.TaskType == TaskTypes.FuelEngines)
            {
                __instance.stage = __instance.Stages[normalPlayerTask.Data[1] % 2];
                __instance.stage.gameObject.SetActive(true);
                __instance.stage.Begin(task);
                Minigame.Instance = __instance;
                UiElement defaultSelection = null;
                foreach (UiElement uiElement in __instance.ControllerSelectable)
                {
                    if (uiElement.isActiveAndEnabled)
                    {
                        defaultSelection = uiElement;
                        break;
                    }
                }
                ControllerManager.Instance.OpenOverlayMenu(
                    __instance.name,
                    __instance.BackButton,
                    defaultSelection,
                    __instance.ControllerSelectable,
                    false
                );
                return false;
            }
            return true;
        }
    }
}