using System.Linq;
using AmongUs.QuickChat;
using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Renames task names and other strings stored as <c>SystemTypes</c>.
/// </summary>
[HarmonyPatch(typeof(QuickChatContext), nameof(QuickChatContext.GetCurrentMapID))]
public static class QuickChatPatch
{
    public static bool Prefix(ref MapNames __result)
    {
        if (!LIShipStatus.IsInstance())
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
        __instance.locations = __instance.locations.AddItem(LIConstants.MAP_STRING_NAME).ToArray();
    }
}

[HarmonyPatch(typeof(QuickChatMapRules), nameof(QuickChatMapRules.Evaluate))]
public static class QuickChatRulesPatch
{
    public static bool Prefix(QuickChatMapRules __instance, ref bool __result)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        __result = __instance.maps.Contains((MapNames)MapType.Skeld);
        return false;
    }
}