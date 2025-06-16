using HarmonyLib;
using LevelImposter.Builders;

namespace LevelImposter.Core;

/// <summary>
///     Normally, moving platforms are handled by
///     AirshipStatus. This bypasses that requirement
///     dependency by supplying it's own platforms.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlatformPatchHandle
{
    public static bool Prefix([HarmonyArgument(0)] byte callId, PlayerControl __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;
        if (!AmongUsClient.Instance.AmHost)
            return true;
        if (callId != (byte)RpcCalls.UsePlatform)
            return true;

        // No platform
        var platform = PlatformBuilder.Platform;
        if (platform == null)
        {
            LILogger.Warn("[RPC] Could not find a moving platform");
            return true;
        }

        // Use Platform
        platform.Use(__instance);
        __instance.SetDirtyBit(4096U);
        return false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
public static class PlatformPatchRPC
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        // No platform
        var platform = PlatformBuilder.Platform;
        if (platform == null)
        {
            LILogger.Warn("[RPC] Could not find a moving platform");
            return true;
        }

        if (AmongUsClient.Instance.AmHost)
            // Use Platform
            platform.Use(__instance);
        else
            // Request Use Platform
            AmongUsClient.Instance.StartRpcImmediately(
                __instance.NetId,
                (byte)RpcCalls.UsePlatform,
                Hazel.SendOption.None
            ).EndMessage();
        return false;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
public static class PlatformResetPatch
{
    public static void Postfix()
    {
        if (!LIShipStatus.IsInstance())
            return;

        LILogger.Info("Meeting Called!");

        // Reset Platform
        PlatformBuilder.Platform?.MeetingCalled();
    }
}