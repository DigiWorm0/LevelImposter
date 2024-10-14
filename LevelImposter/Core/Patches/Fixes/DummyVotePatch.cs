using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Forces dummies to vote with the local player in custom freeplay maps.
/// </summary>
[HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Update))]
public static class DummyVotePatch
{
    public static bool Prefix(DummyBehaviour __instance)
    {
        // Only execute in custom freeplay maps
        if (!LIShipStatus.IsInstance())
            return true;
        if (!GameState.IsInFreeplay)
            return true;

        // Check if the dummy is dead
        var playerData = __instance.myPlayer.Data;
        if (playerData == null || playerData.IsDead)
            return true;

        // Check if we are in a meeting
        if (!GameState.IsInMeeting)
        {
            __instance.voted = false;
        }
        // Check if the dummy has already voted
        else if (!__instance.voted)
        {
            // Check if the local player has voted
            var localPlayerState = MeetingHud.Instance.playerStates[0];
            if (!localPlayerState.DidVote)
                return false;

            // Vote for the same player as the local player
            MeetingHud.Instance.CmdCastVote(playerData.PlayerId, localPlayerState.VotedFor);
            __instance.voted = true;
        }

        return false;
    }
}