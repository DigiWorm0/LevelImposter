using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Uses <c>isActiveAndEnabled</c> as a dependency for <c>MapConsole.CanUse</c>.
///     Needed for enabling/disabling the Admin Table via triggers.
/// </summary>
[HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
public class AdminTablePatch
{
    public static void Postfix(
        Console __instance,
        [HarmonyArgument(1)] ref bool canUse,
        [HarmonyArgument(2)] ref bool couldUse)
    {
        if (LIShipStatus.IsInstance())
            return;

        canUse &= __instance.isActiveAndEnabled;
        couldUse &= __instance.isActiveAndEnabled;
    }
}