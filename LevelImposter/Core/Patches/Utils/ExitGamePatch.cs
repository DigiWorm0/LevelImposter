using HarmonyLib;

namespace LevelImposter.Core.Patches.Utils;

/// <summary>
///     Unloads the map from memory after disconnecting from a game.
/// </summary>
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
public static class ExitGamePatch
{
    public static void Postfix()
    {
        GameConfiguration.SetMap(null);
    }
}