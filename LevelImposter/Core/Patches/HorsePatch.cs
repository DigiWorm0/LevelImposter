namespace LevelImposter.Core
{
    /*
     *      Uncomment to disable the Horse Mod
     */
    /*
    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    public static class HorsePatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
    */
}