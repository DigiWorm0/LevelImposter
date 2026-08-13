using HarmonyLib;
using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;

namespace LevelImposter.Core.Patches.Fixes;

/// <summary>
///     Patches the Emergency Button breaking logic to destroy all buttons on the map.
/// </summary>
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.BreakEmergencyButton))]
public static class EmergencyButtonPatch
{
    public static bool Prefix(ShipStatus __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        var ship = LIShipStatus.GetInstance();

        foreach (var button in ship.transform.GetComponentsInChildren<SystemConsole>())
        {
            button.Image.sprite = __instance.BrokenEmergencyButton;
            button.enabled = false;
        }

        LILogger.Msg("Hooked into ShipStatus.BreakEmergencyButton() and broke all emergency buttons.");

        return false;
    }
}