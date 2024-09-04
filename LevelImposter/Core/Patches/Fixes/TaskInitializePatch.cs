using System.Linq;
using HarmonyLib;
using LevelImposter.Builders;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Normally, tasks have hard-coded counts and limits.
///     In order to allow for flexibility, this has been
///     patched here.
/// </summary>
[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
public static class TaskInitializePatch
{
    public static void Postfix(NormalPlayerTask __instance)
    {
        if (LIShipStatus.IsInstance())
            return;

        var taskType = __instance.TaskType;
        switch (taskType)
        {
            case TaskTypes.ResetBreakers:
                __instance.Data = new byte[TaskConsoleBuilder.BreakerCount];
                for (byte i = 0; i < TaskConsoleBuilder.BreakerCount; i++)
                    __instance.Data[i] = i;
                __instance.Data = MapUtils.Shuffle(__instance.Data);
                __instance.MaxStep = TaskConsoleBuilder.BreakerCount;
                break;
            case TaskTypes.CleanToilet:
                __instance.Data = new byte[1];
                __instance.Data[0] = IntRange.NextByte(0, TaskConsoleBuilder.ToiletCount);
                break;
            case TaskTypes.PickUpTowels:
                var pickupCount = TaskConsoleBuilder.TowelPickupCount ?? TaskConsoleBuilder.TowelCount / 2;
                __instance.Data = new byte[pickupCount];
                var tempData = new byte[TaskConsoleBuilder.TowelCount];
                for (byte i = 0; i < TaskConsoleBuilder.TowelCount; i++)
                    tempData[i] = i;
                tempData = MapUtils.Shuffle(tempData);
                for (byte i = 0; i < __instance.Data.Count; i++)
                    __instance.Data[i] = tempData[i];
                break;
            case TaskTypes.FuelEngines:
                __instance.MaxStep = TaskConsoleBuilder.FuelCount;
                break;
            case TaskTypes.AlignEngineOutput:
                __instance.Data = new byte[TaskConsoleBuilder.AlignEngineCount + 2];
                for (var i = 0; i < TaskConsoleBuilder.AlignEngineCount; i++)
                    __instance.Data[i] = (byte)(IntRange.RandomSign() * IntRange.Next(25, 127) + 127);
                __instance.MaxStep = TaskConsoleBuilder.AlignEngineCount;
                break;
        }
    }
}

/// <summary>
///     <c>WaterWayTask</c> overrides <c>NormalPlayerTask</c> for some reason?
/// </summary>
[HarmonyPatch(typeof(WaterWayTask), nameof(WaterWayTask.Initialize))]
public static class WaterWheelInitializePatch
{
    public static void Postfix(NormalPlayerTask __instance)
    {
        if (LIShipStatus.IsInstance())
            return;

        __instance.Data = new byte[TaskConsoleBuilder.WaterWheelCount];
        __instance.MaxStep = TaskConsoleBuilder.WaterWheelCount;
    }
}

/// <summary>
///     Fix the slider count for Divert Power tasks.
/// </summary>
[HarmonyPatch(typeof(DivertPowerMinigame), nameof(DivertPowerMinigame.Begin))]
public static class DivertBeginPatch
{
    public static void Prefix([HarmonyArgument(0)] PlayerTask task, DivertPowerMinigame __instance)
    {
        if (LIShipStatus.IsInstance())
            return;

        __instance.SliderOrder = ShipTaskBuilder.DivertSystems;
    }
}

/// <summary>
///     Fix the fuel count for Fuel Engines tasks.
/// </summary>
[HarmonyPatch(typeof(MultistageMinigame), nameof(MultistageMinigame.Begin))]
public static class MinigameBeginPatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerTask task, MultistageMinigame __instance)
    {
        if (LIShipStatus.IsInstance())
            return true;

        var normalPlayerTask = task.Cast<NormalPlayerTask>();
        if (normalPlayerTask.TaskType != TaskTypes.FuelEngines)
            return true;

        __instance.stage = __instance.Stages[normalPlayerTask.Data[1] % 2];
        __instance.stage.gameObject.SetActive(true);
        __instance.stage.Begin(task);
        Minigame.Instance = __instance;
        UiElement? defaultSelection = null;
        foreach (var uiElement in __instance.ControllerSelectable)
            if (uiElement.isActiveAndEnabled)
            {
                defaultSelection = uiElement;
                break;
            }

        if (__instance.stage.SkipMultistageOverlayMenuSetup)
            return false;

        __instance.hasOverlayMenu = true;
        ControllerManager.Instance.OpenOverlayMenu(
            __instance.name,
            __instance.BackButton,
            defaultSelection,
            __instance.ControllerSelectable
        );

        return false;
    }
}

/// <summary>
///     Fix the records count for Records tasks.
/// </summary>
[HarmonyPatch(typeof(RecordsMinigame), nameof(RecordsMinigame.GrabFolder))]
public static class RecordsPatch
{
    public static bool Prefix([HarmonyArgument(0)] SpriteRenderer folder, RecordsMinigame __instance)
    {
        if (LIShipStatus.IsInstance())
            return true;

        if (__instance.amClosing != Minigame.CloseState.None)
            return false;

        var folderIndex = 0;
        for (var i = 0; i < __instance.Folders.Count; i++)
            if (__instance.Folders[i].gameObject == folder.gameObject)
                folderIndex = i;

        folder.gameObject.SetActive(false);
        __instance.MyNormTask.Data[folderIndex] = IntRange.NextByte(1, TaskConsoleBuilder.RecordsCount);
        __instance.MyNormTask.UpdateArrowAndLocation();
        if (Constants.ShouldPlaySfx())
            SoundManager.Instance.PlaySound(__instance.grabDocument, false);
        __instance.StartCoroutine(__instance.CoStartClose());

        return false;
    }
}

/// <summary>
///     Fix the hard-coded <c>UpdateArrowAndLocation</c> for Replace Parts tasks.
/// </summary>
[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.UpdateArrowAndLocation))]
public static class PartsPatch
{
    public static bool Prefix(NormalPlayerTask __instance)
    {
        if (LIShipStatus.IsInstance())
            return true;
        if (__instance.TaskType != TaskTypes.ReplaceParts)
            return true;
        if (!__instance.Arrow || !__instance.Owner.AmOwner || __instance.IsComplete)
            return true;

        // Pick Next Console
        var list = NormalPlayerTask.PickRandomConsoles(__instance.taskStep, TaskTypes.ReplaceParts);
        if (list.Count <= 0)
        {
            LILogger.Warn("No consoles found of task-replaceparts2");
            return false;
        }

        // Update Arrow
        __instance.Arrow.target = list[0].transform.position;
        __instance.StartAt = list[0].Room;
        __instance.Data[0] = (byte)list[0].ConsoleId;
        __instance.LocationDirty = true;
        return false;
    }
}

/// <summary>
///     Fixes a bug with <c>NormalPlayerTask.ValidConsole</c> on the fishing task
/// </summary>
[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.ValidConsole))]
public static class FishPatch
{
    public static bool Prefix(
        [HarmonyArgument(0)] Console console,
        NormalPlayerTask __instance,
        ref bool __result)
    {
        if (LIShipStatus.IsInstance())
            return true;
        if (__instance.TaskType != TaskTypes.CatchFish)
            return true;

        // Replace Result
        __result = console.Room == __instance.StartAt &&
                   console.ValidTasks.Any(set =>
                       set.taskType == __instance.TaskType && set.taskStep.Contains(__instance.taskStep)) &&
                   console.TaskTypes.Contains(__instance.TaskType);
        return false;
    }
}