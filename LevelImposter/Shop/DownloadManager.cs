using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Reactor.Networking.Attributes;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    /// <summary>
    /// Handles the download state of all connected clients
    /// </summary>
    public static class DownloadManager
    {
        private static List<PlayerControl> _playersDownloading = new();
        private static string _downloadError = null;

        public static bool CanStart
        {
            get { return _playersDownloading.Count <= 0 && _downloadError == null; }
        }

        /// <summary>
        /// RPC to sync download state across clients
        /// </summary>
        /// <param name="player">Your local PlayerControl</param>
        /// <param name="isDownloaded">TRUE if map is downloaded. FALSE otherwise</param>
        [MethodRpc((uint)LIRpc.DownloadCheck)]
        public static void RPCDownload(PlayerControl player, bool isDownloaded)
        {
            LILogger.Info($"[RPC] {player.name} {(isDownloaded ? "has downloaded" : "is downloading")} the map");
            if (isDownloaded)
                RemovePlayer(player);
            else
                _playersDownloading.Add(player);
        }

        /// <summary>
        /// Removes a player from the downloading list
        /// </summary>
        /// <param name="player">PlayerControl that disconnected</param>
        public static void RemovePlayer(PlayerControl player)
        {
            _playersDownloading.RemoveAll((PlayerControl p) =>
            {
                return p.PlayerId == player.PlayerId;
            });
        }

        /// <summary>
        /// Resets the download state
        /// </summary>
        public static void Reset()
        {
            _playersDownloading.Clear();
            _downloadError = null;
        }

        /// <summary>
        /// Checks whether or not the local player
        /// is currently downloading the map
        /// </summary>
        /// <returns>TRUE if the client is downloading. FALSE otherwise.</returns>
        private static bool IsDownloading()
        {
            foreach (PlayerControl player in _playersDownloading)
                if (player.PlayerId == PlayerControl.LocalPlayer?.PlayerId)
                    return true;
            return false;
        }

        public static void SetError(string error)
        {
            _downloadError = error;
        }

        /// <summary>
        /// Gets the text to display above start button
        /// </summary>
        /// <returns>A string with status text</returns>
        public static string GetStartText()
        {
            if (_downloadError != null)
                return $"<size=4><color=red>{_downloadError}</color></size>";
            else if (IsDownloading())
                return $"<size=4><color=#1a95d8>Downloading map...</color></size>";
            else if (_playersDownloading.Count > 1)
                return $"<size=4><color=#1a95d8>Waiting on </color>{_playersDownloading.Count}<color=#1a95d8> players to download map...</color></size>";
            else if (_playersDownloading.Count == 1)
                return $"<size=4><color=#1a95d8>Waiting on </color>{_playersDownloading[0].name}<color=#1a95d8> to download map...</color></size>";
            else
                return string.Empty;
        }

        /// <summary>
        /// Sends download start RPC
        /// </summary>
        public static void StartDownload()
        {
            MapUtils.WaitForPlayer(() =>
            {
                RPCDownload(PlayerControl.LocalPlayer, false);
            });
        }
        
        /// <summary>
        /// Sends download stop RPC
        /// </summary>
        public static void StopDownload()
        {
            MapUtils.WaitForPlayer(() =>
            {
                RPCDownload(PlayerControl.LocalPlayer, true);
            });
        }
    }
}
