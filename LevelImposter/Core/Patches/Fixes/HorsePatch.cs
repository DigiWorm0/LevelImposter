using HarmonyLib;

namespace LevelImposter.Core
{
#pragma warning disable CS0162 // Uses constants, so ignore unreachable code warning

    /// <summary>
    /// Disables the april-fools horse mode due to incompatibility with lots of mods.
    /// </summary>
    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    public static class HorsePatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (!LIConstants.OVERRIDE_HORSE_MODE)
                return true;

            __result = LIConstants.ENABLE_HORSE_MODE;
            return false;
        }
    }

#pragma warning restore CS0162
}