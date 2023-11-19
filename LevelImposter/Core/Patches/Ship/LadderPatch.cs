using HarmonyLib;
using Hazel;
using LevelImposter.Builders;

namespace LevelImposter.Core
{
    /// <summary>
    /// Normally, ladders are handled by
    /// AirshipStatus. This bypasses that
    /// dependency by supplying it's own
    /// ladder listings.
    /// </summary>
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    public static class LadderPatch
    {
        public static bool Prefix(
            [HarmonyArgument(0)] byte callId,
            [HarmonyArgument(1)] MessageReader reader,
            PlayerPhysics __instance)
        {
            if (LIShipStatus.Instance == null)
                return true;
            if (callId != (byte)RpcCalls.ClimbLadder)
                return true;

            byte ladderId = reader.ReadByte();
            byte climbLadderSid = reader.ReadByte();
            bool isFound = LadderBuilder.TryGetLadder(ladderId, out Ladder? ladder);

            if (isFound)
            {
                __instance.ClimbLadder(ladder, climbLadderSid);
                return false;
            }
            LILogger.Warn($"[RPC] Could not find a ladder of id: {ladderId}");
            return true;
        }
    }
}
