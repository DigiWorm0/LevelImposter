using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LevelImposter.Networking.RPC;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace LevelImposter.Lobby.Sync;

/// <summary>
///     Handles the download state of all connected clients
/// </summary>
public static class PlayersReadyCounter
{
    private static bool _isLocalPlayerReady = true;
    public static List<PlayerControl> NotReadyPlayers { get; } = new();

    /// <summary>
    ///     Sends an RPC indicating whether the local player is ready or still downloading
    /// </summary>
    /// <param name="isReady">True if the player is ready, false if still downloading</param>
    public static void SendPlayerReadyRPC(bool isReady)
    {
        _isLocalPlayerReady = isReady;
        Coroutines.Start(CoSendPlayerReadyRPC());
    }

    private static IEnumerator CoSendPlayerReadyRPC()
    {
        // Wait for local player
        while (PlayerControl.LocalPlayer == null)
            yield return null;

        // Send RPC
        Rpc<ReadyToStartRPC>.Instance.Send(
            PlayerControl.LocalPlayer,
            _isLocalPlayerReady,
            true);
    }

    /// <summary>
    ///     Marks a player as ready
    /// </summary>
    /// <param name="player">The player to mark as ready</param>
    public static void MarkPlayerReady(PlayerControl player)
    {
        var arrayPlayer = NotReadyPlayers.Find(p => p.PlayerId == player.PlayerId);
        if (arrayPlayer != null)
            NotReadyPlayers.Remove(arrayPlayer);
    }

    /// <summary>
    ///     Marks a player as not ready
    /// </summary>
    /// <param name="player">The player to mark as not ready</param>
    public static void MarkPlayerNotReady(PlayerControl player)
    {
        if (NotReadyPlayers.All(p => p.PlayerId != player.PlayerId))
            NotReadyPlayers.Add(player);
    }

    /// <summary>
    ///     Removes any players who's PlayerControls were destroyed
    /// </summary>
    public static void FixNullPlayers()
    {
        NotReadyPlayers.RemoveAll(player => player == null);
    }
}