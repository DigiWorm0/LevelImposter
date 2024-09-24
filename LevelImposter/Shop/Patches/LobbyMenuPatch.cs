using HarmonyLib;

namespace LevelImposter.Shop;

/*
 *      Initializes a new Map Console in the Lobby
 */

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyMenuInitPatch
{
    public static void Postfix()
    {
        LobbyConsoleBuilder.Build();
    }
}