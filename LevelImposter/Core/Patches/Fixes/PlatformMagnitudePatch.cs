using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Bypasses the magnitude maximum when calling <c>MovingPlatformBehaviour.Use</c>.
/// </summary>
[HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.Use), typeof(PlayerControl))]
public static class PlatformMagnitudePatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player, MovingPlatformBehaviour __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;
        if (player.Data.IsDead || player.Data.Disconnected || __instance.Target)
            return true;

        __instance.IsDirty = true;
        __instance.StartCoroutine(__instance.UsePlatform(player));

        return false;
    }
}