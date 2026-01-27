using System;
using LevelImposter.Core;
using LevelImposter.FileIO;
using LevelImposter.Networking;
using Reactor.Networking.Extensions;
using Reactor.Networking.Rpc;

namespace LevelImposter.Lobby;

/// <summary>
/// Helps sync the <see cref="GameConfiguration"/> from the host to all clients. 
/// </summary>
public static class GameConfigurationSync
{
    public static MapDownloadHelper GameMapDownloader { get; private set; } = new(true);
    public static MapDownloadHelper LobbyMapDownloader { get; private set; } = new(false);

    /// <summary>
    /// Sends the current game configuration (map IDs and settings) to all clients in the lobby.
    /// </summary>
    /// <exception cref="Exception">Thrown if the map IDs are not valid GUIDs.</exception>
    public static void SendGameConfigurationRPC()
    {
        if (!PlayerControl.LocalPlayer ||
            !GameState.IsHost ||
            !GameState.IsInLobby)
            return;

        // Get IDs
        var mapIDStr = GameConfiguration.CurrentMap?.id ?? Guid.Empty.ToString();
        if (!Guid.TryParse(mapIDStr, out var mapID))
            throw new Exception($"Invalid map ID [{mapIDStr}]");

        var lobbyMapIDStr = GameConfiguration.CurrentLobbyMap?.id ?? Guid.Empty.ToString();
        if (!Guid.TryParse(lobbyMapIDStr, out var lobbyMapID))
            throw new Exception($"Invalid lobby map ID [{lobbyMapIDStr}]");

        // Transmit RPC
        Rpc<GameConfigurationRPC>.Instance.Send(
            PlayerControl.LocalPlayer, 
            new SerializedGameConfiguration {
                MapID = mapID,
                LobbyMapID = lobbyMapID,
                HideMapName = GameConfiguration.HideMapName
            });
    }

    /// <summary>
    /// Called when a game configuration RPC is received from the host.
    /// </summary>
    /// <param name="gameConfig">The received serialized game configuration.</param>
    public static void OnGameConfigurationRPC(SerializedGameConfiguration gameConfig)
    {
        // Cancel Game Start
        if (DestroyableSingleton<GameStartManager>.InstanceExists)
            GameStartManager.Instance.ResetStartState();

        // Update Lobby Map
        UpdateLobbyMap(gameConfig);

        // Update Game Map
        UpdateGameMap(gameConfig);
    }

    /// <summary>
    /// Updates the lobby map based on the received game configuration.
    /// </summary>
    /// <param name="gameConfig">The received serialized game configuration.</param>
    private static void UpdateLobbyMap(SerializedGameConfiguration gameConfig)
    {
        var mapIDStr = gameConfig.LobbyMapID.ToString();

        // Cancel any other active download for different map
        if (!LobbyMapDownloader.IsDownloadingMap(gameConfig.LobbyMapID))
            LobbyMapDownloader.CancelDownload();

        // This map is already loaded
        if (GameConfiguration.CurrentLobbyMap?.id == mapIDStr)
        {
            // No action needed
        }
        // Try to get map locally
        else if (TryGetMapLocally(mapIDStr, out var map))
        {
            GameConfiguration.SetLobbyMap(map);
        }
        // Download map if not found locally
        else
        {
            GameConfiguration.SetLobbyMap(null);
            LobbyMapDownloader.DownloadMap(
                gameConfig.LobbyMapID,
                map => GameConfiguration.SetLobbyMap(map),

                // Don't allow players to be in the lobby without the active lobby map loaded
                error => AmongUsClient.Instance.DisconnectWithReason($"Failed to download lobby map: {error}")
            );
        }
    }

    /// <summary>
    /// Updates the game map based on the received game configuration.
    /// </summary>
    /// <param name="gameConfig">The received serialized game configuration.</param>
    private static void UpdateGameMap(SerializedGameConfiguration gameConfig)
    {
        var mapIDStr = gameConfig.MapID.ToString();

        // Cancel any other active download for different map
        if (!GameMapDownloader.IsDownloadingMap(gameConfig.MapID))
            GameMapDownloader.CancelDownload();

        // This map is already loaded
        if (GameConfiguration.CurrentMap?.id == mapIDStr)
        {
            // No action needed
        }
        // Try to get map locally
        else if (TryGetMapLocally(mapIDStr, out var map))
        {
            GameConfiguration.SetMap(map, gameConfig.HideMapName);
        }
        // Download map if not found locally
        else
        {
            GameConfiguration.SetMap(null);
            GameMapDownloader.DownloadMap(
                gameConfig.MapID,
                map => GameConfiguration.SetMap(map, gameConfig.HideMapName),
                _ => {}
            );
            return;
        }
    }

    /// <summary>
    /// Tries to get the map locally from filesystem or cache.
    /// </summary>
    /// <param name="mapID">The map ID to look for.</param>
    /// <param name="map">The found map, or null if not found.</param>
    /// <returns>True if the map was found locally; otherwise, false.</returns>
    private static bool TryGetMapLocally(string mapID, out LIMap? map)
    {
        // No Map Selected
        if (mapID == Guid.Empty.ToString())
        {
            map = null;
            return true; // <-- This is a valid state, we return true
        }

        // In Local Filesystem
        if (MapFileAPI.Exists(mapID))
        {
            map = MapFileAPI.Get(mapID);
            if (map == null)
                LILogger.Warn($"Error loading [{mapID}] from filesystem");
            else
                return true;
        }

        // In Local Cache
        if (MapFileCache.Exists(mapID))
        {
            map = MapFileCache.Get(mapID);
            if (map == null)
                LILogger.Warn($"Error loading [{mapID}] from cache");
            else
                return true;
        }

        // Couldn't find map locally
        map = null;
        return false;
    }
}