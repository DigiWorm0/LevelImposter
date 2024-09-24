using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Sets Ladder Cooldown to editable value
/// </summary>
[HarmonyPatch(typeof(Ladder), nameof(Ladder.SetDestinationCooldown))]
public static class DestinationLadderCooldownPatch
{
    public static void Postfix(Ladder __instance)
    {
        if (__instance is not EditableLadderConsole editableLadder)
            return;

        __instance.Destination.CoolDown = editableLadder.MaxCoolDown;
    }
}

[HarmonyPatch(typeof(Ladder), nameof(Ladder.Use))]
public static class LadderCooldownPatch
{
    public static void Postfix(Ladder __instance)
    {
        if (__instance is not EditableLadderConsole editableLadder)
            return;

        if (__instance.CoolDown == __instance.MaxCoolDown)
            __instance.CoolDown = editableLadder.MaxCoolDown;
    }
}

[HarmonyPatch(typeof(Ladder))]
[HarmonyPatch(nameof(Ladder.PercentCool), MethodType.Getter)]
public static class LadderPercentCooldownPatch
{
    public static bool Prefix(Ladder __instance, out float __result)
    {
        __result = 0;
        if (__instance is not EditableLadderConsole editableLadder)
            return true;

        __result = __instance.CoolDown / editableLadder.MaxCoolDown;
        return false;
    }
}