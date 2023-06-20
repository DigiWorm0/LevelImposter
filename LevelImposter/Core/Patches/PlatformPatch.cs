using HarmonyLib;
using Hazel;
using LevelImposter.Builders;

namespace LevelImposter.Core
{
    /*
     *      Normally, moving platforms are handled
     *      by AirshipStatus. This bypasses that
     *      requirement by supplying it's own
     *      platform listings.
     */
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class PlatformPatchHandle
    {
        public static bool Prefix([HarmonyArgument(0)] byte callId, PlayerControl __instance)
        {
            if (LIShipStatus.Instance == null)
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
            if (LIShipStatus.Instance == null)
                return true;

            // No platform
            var platform = PlatformBuilder.Platform;
            if (platform == null)
            {
                LILogger.Warn("[RPC] Could not find a moving platform");
                return true;
            }

            if (AmongUsClient.Instance.AmHost)
            {
                // Use Platform
                platform.Use(__instance);
            }
            else
            {
                // Request Use Platform
                AmongUsClient.Instance.StartRpc(
                    __instance.NetId,
                    (byte)RpcCalls.UsePlatform,
                    SendOption.Reliable
                ).EndMessage();
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
    public static class PlatformResetPatch
    {
        public static void Postfix()
        {
            if (LIShipStatus.Instance == null)
                return;

            LILogger.Info("Meeting Called!");

            // Reset Platform
            PlatformBuilder.Platform?.MeetingCalled();
        }
    }

    /*
     *      This removes the magnitude maximum
     *      when using a Moving Platform
     */
    [HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.Use), typeof(PlayerControl))]
    public static class PlatformUsePatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl player, MovingPlatformBehaviour __instance)
        {
            if (LIShipStatus.Instance == null)
                return true;
            if (player.Data.IsDead || player.Data.Disconnected || __instance.Target)
                return true;

            __instance.IsDirty = true;
            __instance.StartCoroutine(__instance.UsePlatform(player));

            return false;
        }
    }
}
