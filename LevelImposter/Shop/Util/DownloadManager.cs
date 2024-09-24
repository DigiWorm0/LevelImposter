using System.Collections.Generic;
using System.Linq;
using LevelImposter.Core;
using Reactor.Networking.Attributes;

namespace LevelImposter.Shop;

/// <summary>
///     Handles the download state of all connected clients
/// </summary>
public static class DownloadManager
{
    private static readonly List<PlayerControl> _playersDownloading = new();
    private static string? _downloadError;
    private static int _downloadPercent;

    public static bool CanStart => _playersDownloading.Count <= 0 && string.IsNullOrEmpty(_downloadError);

    /// <summary>
    ///     RPC to sync download state across clients
    /// </summary>
    /// <param name="player">Your local PlayerControl</param>
    /// <param name="isDownloaded">TRUE if map is downloaded. FALSE otherwise</param>
    [MethodRpc((uint)LIRpc.DownloadCheck)]
    public static void RPCDownload(PlayerControl player, bool isDownloaded)
    {
        LILogger.Info($"[RPC] {player.name} {(isDownloaded ? "has downloaded" : "is downloading")} the map");
        if (DestroyableSingleton<GameStartManager>.InstanceExists)
            DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
        if (isDownloaded)
            RemovePlayer(player);
        else
            AddPlayer(player);
    }

    /// <summary>
    ///     Removes a player from the downloading list
    /// </summary>
    /// <param name="player">PlayerControl that disconnected</param>
    public static void RemovePlayer(PlayerControl player)
    {
        _playersDownloading.RemoveAll(p => p.PlayerId == player.PlayerId);
    }

    /// <summary>
    ///     Adds a player to the downloading list
    /// </summary>
    /// <param name="player">PlayerControl that connected</param>
    public static void AddPlayer(PlayerControl player)
    {
        if (_playersDownloading.All(p => p.PlayerId != player.PlayerId))
            _playersDownloading.Add(player);
    }

    /// <summary>
    ///     Resets the download state
    /// </summary>
    public static void Reset()
    {
        _playersDownloading.Clear();
        _downloadError = null;
    }

    /// <summary>
    ///     Checks whether the local player
    ///     is currently downloading the map
    /// </summary>
    /// <returns>TRUE if the client is downloading. FALSE otherwise.</returns>
    private static bool IsDownloading()
    {
        return _playersDownloading.Any(player => player.PlayerId == PlayerControl.LocalPlayer?.PlayerId);
    }

    /// <summary>
    ///     Sets the download progress
    /// </summary>
    /// <param name="percent">Progress between 0 and 1 (inclusive)</param>
    public static void SetProgress(float percent)
    {
        _downloadPercent = (int)(percent * 100);
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
            return $"DOWNLOADING MAP ({_downloadPercent}%)";

        return _playersDownloading.Count switch
        {
            > 1 =>
                $"WAITING ON <color=#1a95d8>{_playersDownloading.Count} players</color> TO DOWNLOAD MAP",
            1 =>
                $"WAITING ON <color=#1a95d8>{_playersDownloading[0].name}</color> TO DOWNLOAD MAP",
            _ => string.Empty
        };
    }

    /// <summary>
    ///     Sends download start RPC
    /// </summary>
    public static void StartDownload()
    {
        _downloadPercent = 0;
        MapUtils.WaitForPlayer(() => { RPCDownload(PlayerControl.LocalPlayer, false); });
    }

    /// <summary>
    ///     Sends download stop RPC
    /// </summary>
    public static void StopDownload()
    {
        _downloadPercent = 0;
        MapUtils.WaitForPlayer(() => { RPCDownload(PlayerControl.LocalPlayer, true); });
    }
}