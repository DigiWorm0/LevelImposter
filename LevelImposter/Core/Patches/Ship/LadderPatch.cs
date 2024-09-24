using HarmonyLib;
using Hazel;
using LevelImposter.Builders;

namespace LevelImposter.Core;

/// <summary>
///     Normally, ladders are handled by
///     AirshipStatus. This bypasses that
///     dependency by supplying it's own
///     ladder listings.
/// </summary>
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
public static class LadderPatch
{
    public static bool Prefix(
        [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader,
        PlayerPhysics __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;
        if (callId != (byte)RpcCalls.ClimbLadder)
            return true;

        var ladderId = reader.ReadByte();
        var climbLadderSid = reader.ReadByte();
        var isFound = LadderBuilder.TryGetLadder(ladderId, out var ladder);

        if (isFound)
        {
            __instance.ClimbLadder(ladder, climbLadderSid);
            return false;
        }

        LILogger.Warn($"[RPC] Could not find a ladder of id: {ladderId}");
        return true;
    }
}