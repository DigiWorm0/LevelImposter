using System;
using HarmonyLib;
using LevelImposter.Core.Components;

namespace LevelImposter.Core.Patches.Fixes;

/// <summary>
///     Removes the minimum task limit on <c>ShipStatus</c>.
/// </summary>
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
public class MinimumTaskPatch
{
    public static void Prefix(ShipStatus __instance)
    {
        if (!LIShipStatus.IsInstance())
            return;

        // Get Counts
        var shortTaskCount = __instance.ShortTasks.Count;
        var longTaskCount = __instance.LongTasks.Count;
        var commonTaskCount = __instance.CommonTasks.Count;

        // Update Game Options
        var currentOptions = GameOptionsManager.Instance.currentNormalGameOptions;
        if (currentOptions == null)
            return; // <-- Other gamemodes (ex: Hide & Seek)

        currentOptions.NumShortTasks = Math.Min(currentOptions.NumShortTasks, shortTaskCount);
        currentOptions.NumLongTasks = Math.Min(currentOptions.NumLongTasks, longTaskCount);
        currentOptions.NumCommonTasks = Math.Min(currentOptions.NumCommonTasks, commonTaskCount);
    }
}