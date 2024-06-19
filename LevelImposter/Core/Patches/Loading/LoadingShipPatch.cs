using HarmonyLib;
using InnerNet;

namespace LevelImposter.Core
{
    /// <summary>
    /// Waits for the ship to finish loading before sending the client ready packet.
    /// </summary>
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendClientReady))]
    public static class LoadingShipPatch
    {
        public static bool Prefix(InnerNetClient __instance)
        {
            // Continue if ship is already loaded
            if (LIShipStatus.Instance == null || LIShipStatus.Instance.IsReady)
                return true;

            // Wait for ship to finish loading, then send packet
            MapUtils.WaitForShip(LIConstants.MAX_LOAD_TIME, __instance.SendClientReady);

            // Don't send packet
            return false;
        }
    }

    /// <summary>
    /// Increases the maximum wait time for all clients to load the ship.
    /// Normally, this is 10 seconds, but it is increased to 20 seconds.
    /// </summary>
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class LoadingShipTimerPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            // TODO: Fix me
            //AmongUsClient.MAX_CLIENT_WAIT_SECONDS = LIConstants.MAX_HOST_TIMEOUT; // TODO: Fix Max Client Timeout Adjust
        }
    }
}
