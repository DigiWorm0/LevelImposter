using AmongUs.QuickChat;
using HarmonyLib;
using System.Linq;

namespace LevelImposter.Core
{
    /// <summary>
    /// Renames task names and other strings stored as <c>SystemTypes</c>.
    /// </summary>
    [HarmonyPatch(typeof(QuickChatContext), nameof(QuickChatContext.GetCurrentMapID))]
    public static class QuickChatPatch
    {
        public static bool Prefix(ref MapNames __result)
        {
            if (LIShipStatus.Instance == null)
                return true;

            __result = (MapNames)MapType.LevelImposter;
            return false;
        }
    }
    [HarmonyPatch(typeof(QuickChatContext), nameof(QuickChatContext.UpdateWithCurrentLobby))]
    public static class QuickChatMapPatch
    {
        public static void Postfix(QuickChatContext __instance)
        {
            __instance.locations = CollectionExtensions.AddItem(__instance.locations, LIConstants.MAP_STRING_NAME).ToArray();
        }
    }
    [HarmonyPatch(typeof(QuickChatMapRules), nameof(QuickChatMapRules.Evaluate))]
    public static class QuickChatRulesPatch
    {
        public static bool Prefix(QuickChatMapRules __instance, ref bool __result)
        {
            if (LIShipStatus.Instance == null)
                return true;

            __result = __instance.maps.Contains((MapNames)MapType.Skeld);
            return false;
        }
    }
}