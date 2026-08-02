using HarmonyLib;
using LevelImposter.Core.Models;

namespace LevelImposter.Core.Patches.Utils;

/// <summary>
///     Decreases the minimum player count to start the game.
/// </summary>
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class MinPlayerPatch
{
    public static void Postfix(GameStartManager __instance)
    {
        if (LIConstants.IS_DEVELOPMENT_BUILD)
            __instance.MinPlayers = 1;
    }
}

/// <summary>
///     Disables the end game condition check.
/// </summary>
[HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
public static class EndGamePatch
{
    public static bool Prefix()
    {
        return !LIConstants.IS_DEVELOPMENT_BUILD;
    }
}