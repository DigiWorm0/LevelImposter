using HarmonyLib;
using LevelImposter.Lobby.Sync;

/*
 *      Cancels any map downloads if the player disconnects from the lobby/game.
 */
namespace LevelImposter.Lobby.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
public static class LobbyCancelDownloadsPatch
{
    public static void Postfix()
    {
        GameConfigurationSync.CancelAllDownloads();
    }
}