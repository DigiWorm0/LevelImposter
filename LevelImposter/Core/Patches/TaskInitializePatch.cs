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
            }
        }
    }
}