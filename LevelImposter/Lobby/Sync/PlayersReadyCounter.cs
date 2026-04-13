using System.Collections.Generic;
using LevelImposter.Networking;
using Reactor.Networking.Rpc;

namespace LevelImposter.Lobby;

/// <summary>
///     Handles the download state of all connected clients
/// </summary>
public static class PlayersReadyCounter
{
    public static List<PlayerControl> NotReadyPlayers { get; } = new();

    /// <summary>
    ///     Sends an RPC indicating whether the local player is ready or still downloading
    /// </summary>
    /// <param name="isReady">True if the player is ready, false if still downloading</param>
    public static void SendPlayerReadyRPC(bool isReady)
    {
        Rpc<ReadyToStartRPC>.Instance.Send(isReady, true);
    }

    /// <summary>
    ///     Marks a player as ready
    /// </summary>
    /// <param name="player">The player to mark as ready</param>
    public static void MarkPlayerReady(PlayerControl player)
    {
        if (NotReadyPlayers.Contains(player))
            NotReadyPlayers.Remove(player);
    }

    /// <summary>
    ///     Marks a player as not ready
    /// </summary>
    /// <param name="player">The player to mark as not ready</param>
    public static void MarkPlayerNotReady(PlayerControl player)
    {
        if (!NotReadyPlayers.Contains(player))
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