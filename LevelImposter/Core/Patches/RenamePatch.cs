using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;

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
            if (MapLoader.currentMap == null)
                return true;

            if (MapUtils.systemRenames.ContainsKey(systemType))
            {
                __result = MapUtils.systemRenames[systemType];
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
            if (MapLoader.currentMap == null)
                return true;

            if (MapUtils.taskRenames.ContainsKey(taskType))
            {
                __result = MapUtils.taskRenames[taskType];
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.ToHudString))]
    public static class StringRenamePatch
    {
        public static void Postfix(GameOptionsData __instance, ref string __result)
        {
            if (MapLoader.currentMap == null)
                return;

            int mapID = (int)((__instance.MapId == 0 && Constants.ShouldFlipSkeld()) ? 3 : __instance.MapId);
            string oldMapName = Constants.MapNames[mapID];
            __result = __result.Replace(oldMapName, MapLoader.currentMap.name);
        }
    }
}