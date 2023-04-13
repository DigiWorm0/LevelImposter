using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;
using AmongUs.GameOptions;

namespace LevelImposter.Core
{
    /*
     *      Renames systems based on a
     *      LI map's renamed value's
     */
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new System.Type[] { typeof(SystemTypes) })]
    public static class SystemRenamePatch
    {
        public static bool Prefix([HarmonyArgument(0)] SystemTypes systemType, ref string __result)
        {
            if (LIShipStatus.Instance == null || !LIShipStatus.Instance.Renames.Contains(systemType))
                return true;

            __result = LIShipStatus.Instance.Renames.Get(systemType);
            return false;
        }
    }
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new System.Type[] { typeof(TaskTypes) })]
    public static class TaskRenamePatch
    {
        public static bool Prefix([HarmonyArgument(0)] TaskTypes taskType, ref string __result)
        {
            if (LIShipStatus.Instance == null || !LIShipStatus.Instance.Renames.Contains(taskType))
                return true;

            __result = LIShipStatus.Instance.Renames.Get(taskType);
            return false;
        }
    }
    
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    public static class StringRenamePatch
    {
        public static void Postfix(ref string __result)
        {
            if (MapLoader.CurrentMap == null)
                return;

            __result = __result.Replace(LIConstants.MAP_NAME, MapLoader.CurrentMap.name);
        }
    }
}