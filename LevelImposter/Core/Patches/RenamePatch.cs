using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
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
            if (MapLoader.CurrentMap == null)
                return true;

            if (MapUtils.SystemRenames.ContainsKey(systemType))
            {
                __result = MapUtils.SystemRenames[systemType];
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new System.Type[] { typeof(TaskTypes) })]
    public static class TaskRenamePatch
    {
        public static bool Prefix([HarmonyArgument(0)] TaskTypes taskType, ref string __result)
        {
            if (MapLoader.CurrentMap == null)
                return true;

            if (MapUtils.TaskRenames.ContainsKey(taskType))
            {
                __result = MapUtils.TaskRenames[taskType];
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    public static class StringRenamePatch
    {
        public static void Postfix([HarmonyArgument(0)] IGameOptions gameOptions, ref string __result)
        {
            if (MapLoader.CurrentMap == null)
                return;

            int mapID = (gameOptions.MapId == 0 && Constants.ShouldFlipSkeld()) ? 3 : gameOptions.MapId;
            string oldMapName = Constants.MapNames[mapID];
            __result = __result.Replace(oldMapName, MapLoader.CurrentMap.name);
        }
    }
}