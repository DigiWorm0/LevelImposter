using HarmonyLib;
using LevelImposter.Builders;

namespace LevelImposter.Core;

/// <summary>
///     Normally, mushroom mixup is handled by
///     FungleShipStatus. This bypasses that
///     dependency by supplying it's own system.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.IsMushroomMixupActive))]
public static class MixupPatch
{
    public static bool Prefix(PlayerControl __instance, ref bool __result)
    {
        if (LIShipStatus.IsInstance())
            return true;

        __result = (SabMixupBuilder.SabotageSystem?.IsActive ?? false) ||
                   __instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup;
        return false;
    }
}