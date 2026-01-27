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
    /// Sends an RPC indicating whether the local player is ready or still downloading
    /// </summary>
    /// <param name="isReady">True if the player is ready, false if still downloading</param>
    public static void SendPlayerReadyRPC(bool isReady)
    {
        Rpc<ReadyToStartRPC>.Instance.Send(isReady, true);
    }

    /// <summary>
    /// Marks a player as ready
    /// </summary>
    /// <param name="player">The player to mark as ready</param>
    public static void MarkPlayerReady(PlayerControl player)
    {
        if (NotReadyPlayers.Contains(player))
            NotReadyPlayers.Remove(player);
    }

    /// <summary>
    ///   Marks a player as not ready
    /// </summary>
    /// <param name="player">The player to mark as not ready</param>
    public static void MarkPlayerNotReady(PlayerControl player)
    {
        if (!NotReadyPlayers.Contains(player))
            NotReadyPlayers.Add(player);
    }

    /// <summary>
    ///     Gets the text to display above start button
    /// </summary>
    /// <returns>A string with status text</returns>
    public static string GetStartText()
    {
        // Check local download state
        var mapDownloadState = GameConfigurationSync.GameMapDownloader.CurrentDownloadState;
        if (mapDownloadState?.Error != null)
            return $"ERROR: {mapDownloadState?.Error}";
        if (mapDownloadState != null)
            return $"DOWNLOADING MAP ({mapDownloadState.Progress * 100:F1}%)";

        // Check other players
        return NotReadyPlayers.Count switch
        {
            > 1 => $"WAITING ON <color=#1a95d8>{NotReadyPlayers.Count} players</color> TO DOWNLOAD MAP",
            1 => $"WAITING ON <color=#1a95d8>{NotReadyPlayers[0].name}</color> TO DOWNLOAD MAP",
            _ => string.Empty
        };
    }
}