using System.Collections.Generic;
using System.Linq;
using LevelImposter.Core;
using LevelImposter.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Lobby;

/// <summary>
///     Handles the download state of all connected clients
/// </summary>
public static class DownloadManager
{
    private static readonly List<PlayerControl> PlayersDownloading = new();
    private static string? _downloadError;
    
    public static int DownloadPercent { get; private set; }
    public static bool CanStart => PlayersDownloading.Count == 0 && string.IsNullOrEmpty(_downloadError);

    /// <summary>
    /// Syncs the download state of a player to all clients
    /// </summary>
    /// <param name="player">PlayerControl to sync</param>
    /// <param name="isDownloaded">TRUE if the player has downloaded the map, FALSE if they are downloading</param>
    private static void SyncDownloadState(PlayerControl player, bool isDownloaded)
    {
        Rpc<DownloadCheckRPC>.Instance.Send(player, isDownloaded, true);
    }

    /// <summary>
    ///     Removes a player from the downloading list
    /// </summary>
    /// <param name="player">PlayerControl that disconnected</param>
    public static void RemovePlayer(PlayerControl player)
    {
        PlayersDownloading.RemoveAll(p => p.PlayerId == player.PlayerId);
    }

    /// <summary>
    ///     Adds a player to the downloading list
    /// </summary>
    /// <param name="player">PlayerControl that connected</param>
    public static void AddPlayer(PlayerControl player)
    {
        if (PlayersDownloading.All(p => p.PlayerId != player.PlayerId))
            PlayersDownloading.Add(player);
    }

    /// <summary>
    ///     Resets the download state
    /// </summary>
    public static void Reset()
    {
        PlayersDownloading.Clear();
        _downloadError = null;
    }

    /// <summary>
    ///     Checks whether the local player
    ///     is currently downloading the map
    /// </summary>
    /// <returns>TRUE if the client is downloading. FALSE otherwise.</returns>
    private static bool IsDownloading()
    {
        return PlayersDownloading.Any(player => player.PlayerId == PlayerControl.LocalPlayer?.PlayerId);
    }

    /// <summary>
    ///     Sets the download progress
    /// </summary>
    /// <param name="percent">Progress between 0 and 1 (inclusive)</param>
    public static void SetProgress(float percent)
    {
        DownloadPercent = (int)(percent * 100);
    }

    /// <summary>
    ///     Sets the error text
    /// </summary>
    /// <param name="error">Text to display on error</param>
    public static void SetError(string error)
    {
        _downloadError = error;
    }

    /// <summary>
    ///     Gets the text to display above start button
    /// </summary>
    /// <returns>A string with status text</returns>
    public static string GetStartText()
    {
        if (_downloadError != null)
            return $"ERROR: {_downloadError}";
        if (IsDownloading())
            return $"DOWNLOADING MAP ({DownloadPercent}%)";

        return PlayersDownloading.Count switch
        {
            > 1 => $"WAITING ON <color=#1a95d8>{PlayersDownloading.Count} players</color> TO DOWNLOAD MAP",
            1 => $"WAITING ON <color=#1a95d8>{PlayersDownloading[0].name}</color> TO DOWNLOAD MAP",
            _ => string.Empty
        };
    }

    /// <summary>
    ///     Sends download start RPC
    /// </summary>
    public static void StartDownload()
    {
        DownloadPercent = 0;
        MapUtils.WaitForPlayer(() => { SyncDownloadState(PlayerControl.LocalPlayer, false); });
    }

    /// <summary>
    ///     Sends download stop RPC
    /// </summary>
    public static void StopDownload()
    {
        DownloadPercent = 0;
        MapUtils.WaitForPlayer(() => { SyncDownloadState(PlayerControl.LocalPlayer, true); });
    }
}