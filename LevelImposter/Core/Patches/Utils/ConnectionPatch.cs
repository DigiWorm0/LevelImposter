using HarmonyLib;
using Hazel.Udp;

namespace LevelImposter.Core
{
    /// <summary>
    /// Increases the max timeout for the low level connection to the server.
    /// </summary>
    [HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.ConnectAsync))]
    public static class TimeoutPatch
    {
        public static void Postfix(UnityUdpClientConnection __instance)
        {
            __instance.DisconnectTimeoutMs = LIConstants.MAX_CONNECTION_TIMEOUT * 1000;
        }
    }
}
