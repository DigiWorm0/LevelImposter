using HarmonyLib;
using LevelImposter.Builders;

namespace LevelImposter.Core
{
    /*
     *      Normally, mushroom mixup is handled
     *      by FungleShipStatus. This bypasses that
     *      requirement by supplying it's own system.
     */
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.IsMushroomMixupActive))]
    public static class MixupPatch
    {
        public static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            if (LIShipStatus.Instance == null)
                return true;

            __result = (SabMixupBuilder.SabotageSystem?.IsActive ?? false) ||
                __instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup;
            return false;
        }
    }
}
