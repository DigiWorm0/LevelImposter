
using HarmonyLib;
using InnerNet;
using LevelImposter.Lobby;

/*
 *      Remove player from PlayersReadyCounter
 *      if the player disconnects
 */
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public static class LobbyPlayerDisconnectPatch
{
    public static void Postfix([HarmonyArgument(0)] ClientData data)
    {
        if (data.Character != null)
            PlayersReadyCounter.MarkPlayerReady(data.Character);
    }
}