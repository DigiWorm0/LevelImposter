using HarmonyLib;
using LevelImposter.Build.Builders.Util;
using LevelImposter.Core.Components;

namespace LevelImposter.Core.Patches.Fixes;

/// <summary>
///     Fixes the <c>ShipStatus.BreakEmergencyButton</c> method to
///     allow for multiple buttons placed throughout the map.
/// </summary>
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.BreakEmergencyButton))]
public class BreakEmergencyButtonPatch
{
    public static bool Prefix(ShipStatus __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        foreach (var button in UtilBuilder.AllEmergencyButtons)
        {
            button.Image.sprite = __instance.BrokenEmergencyButton;
            button.enabled = false;
        }

        return false;
    }
}