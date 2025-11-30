using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Fixes a bug where the ExileController.completeString would reference
///     undefined memory when overloaded by LIExileController.
/// </summary>
[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public class ExileTextPatch
{
    public static string LastExileText { get; private set; } = string.Empty;
    
    public static void Postfix(ExileController __instance)
    {
        if (!LIShipStatus.IsInstance())
            return;

        LastExileText = __instance.completeString;
    }
}