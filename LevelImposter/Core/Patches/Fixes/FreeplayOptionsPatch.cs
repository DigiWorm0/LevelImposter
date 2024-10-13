using System;
using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Changes the freeplay settings on LI maps.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class FreeplayOptionsPatch
{
    public static void Postfix()
    {
        // Only execute in custom freeplay maps
        if (!LIShipStatus.IsInstance())
            return;
        if (!GameState.IsInFreeplay)
            return;

        // Set the freeplay settings
        var gameManager = GameManager.Instance;
        var gameOptions = gameManager.LogicOptions.TryCast<LogicOptionsNormal>();
        if (gameOptions == null)
            throw new Exception("Failed to cast game options to NormalGameOptionsV07");

        gameOptions.GameOptions.NumEmergencyMeetings = 5;
        gameOptions.GameOptions.DiscussionTime = 0;
        gameOptions.GameOptions.EmergencyCooldown = 0;
        gameOptions.GameOptions.VotingTime = 20;
        gameOptions.GameOptions.KillCooldown = 5;
    }
}