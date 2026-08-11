using HarmonyLib;

namespace LevelImposter.Core.Patches.Utils;

#if DEBUG
/// <summary>
///     Decreases the minimum player count to start the game.
/// </summary>
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class MinPlayerPatch
{
    public static void Postfix(GameStartManager __instance)
    {
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
        return false;
    }
}
#endif