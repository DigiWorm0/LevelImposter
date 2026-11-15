using HarmonyLib;
using LevelImposter.Core.Utils;

namespace LevelImposter.Core.Patches.Utils;

/// <summary>
///     Renders logged errors as in-game notifications while in Freeplay.
/// </summary>
[HarmonyPatch(typeof(LILogger), nameof(LILogger.Error))]
public static class ErrorNotifyPatch
{
    public static void Postfix(object data)
    {
        if (GameState.IsInFreeplay)
            LINotifier.Notify(data.ToString() ?? "null");
    }
}

/// <summary>
///     Renders logged warnings as in-game notifications while in Freeplay.
/// </summary>
[HarmonyPatch(typeof(LILogger), nameof(LILogger.Warn))]
public static class WarnNotifyPatch
{
    public static void Postfix(object data)
    {
        if (GameState.IsInFreeplay)
            LINotifier.Notify("<color=yellow>" + data + "</color>");
    }
}
