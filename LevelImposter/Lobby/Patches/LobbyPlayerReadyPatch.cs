using HarmonyLib;
using InnerNet;
using LevelImposter.Lobby.Sync;

/*
 *      Remove player from PlayersReadyCounter
 *      if the player disconnects
 */
namespace LevelImposter.Lobby.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public static class LobbyPlayerDisconnectPatch
{
    public static void Postfix([HarmonyArgument(0)] ClientData data)
    {
        if (data.Character != null)
            PlayersReadyCounter.MarkPlayerReady(data.Character);
    }
}