using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Disable SFX during map loading sequence.
/// </summary>
[HarmonyPatch(typeof(Constants), nameof(Constants.ShouldPlaySfx))]
public class SFXLoadingPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (!LIShipStatus.IsInstance())
            return true;
        if (LIShipStatus.IsReady)
            return true;

        // Prevent SFX from playing
        __result = false;
        return false;
    }
}