using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;
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
        public static bool Prefix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader, PlayerControl __instance)
        {
            if (MapLoader.CurrentMap == null)
                return true;

            if (callId == 32 && AmongUsClient.Instance.AmHost)
            {
                if (PlatformBuilder.Platform != null)
                {
                    PlatformBuilder.Platform.Use(__instance);
                    __instance.SetDirtyBit(4096U);
                    return false;
                }
                LILogger.Warn("[RPC] Could not find a moving platform");
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
    public static class PlatformPatchRPC
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (MapLoader.CurrentMap == null)
                return true;

            if (AmongUsClient.Instance.AmHost)
            {
                if (PlatformBuilder.Platform != null)
                {
                    PlatformBuilder.Platform.Use(__instance);
                    __instance.SetDirtyBit(4096U);
                }
                else
                {
                    LILogger.Warn("[RPC] Could not find a moving platform");
                }
            }
            return true;
        }
    }
}
