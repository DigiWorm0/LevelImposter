using System.Collections.Generic;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Components;
using LevelImposter.Lobby.Patches;
using UnityEngine;

namespace LevelImposter.Builders.Lobby;

public class LobbySpawnBuilder : IElemBuilder
{
    public static readonly List<LobbySpawnPoint> SpawnPositions = [];

    public void OnPreBuild()
    {
        SpawnPositions.Clear();
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-lobbyspawn")
            return;

        // Add to local list
        SpawnPositions.Add(new LobbySpawnPoint
        {
            IsFlipped = obj.transform.localScale.x < 0,
            Position = obj.transform.position
        });

        // Add to Lobby Behavior list
        var lobbyBehaviour = LILobbyBehaviour.GetLobbyBehaviour();
        lobbyBehaviour.SpawnPositions = lobbyBehaviour.SpawnPositions.Add(obj.transform.position);
    }

    public void OnPostBuild()
    {
        // Check if no spawn positions were added
        var lobbyBehaviour = LILobbyBehaviour.GetLobbyBehaviour();
        if (lobbyBehaviour.SpawnPositions.Length == 0)
            lobbyBehaviour.SpawnPositions = new[] { Vector2.zero };

        if (SpawnPositions.Count == 0)
            SpawnPositions.Add(new LobbySpawnPoint
            {
                IsFlipped = false,
                Position = Vector2.zero
            });

        // Replay the spawn animations for all players
        foreach (var playerControl in PlayerControl.AllPlayerControls)
        {
            // Stop spawning player
            var spawnCoroutine = LobbySpawnPatch.SpawnCoroutines.GetValueOrDefault(playerControl.PlayerId);
            if (spawnCoroutine != null)
                playerControl.StopCoroutine(spawnCoroutine);

            // Start spawning player
            playerControl.StartCoroutine(
                playerControl.MyPhysics.CoSpawnPlayer(lobbyBehaviour)
            );
        }
    }

    /// <summary>
    ///     Gets the spawn point of a given player
    /// </summary>
    /// <param name="playerControl">The player to spawn in</param>
    /// <returns>A LobbySpawnPoint containing the target player's spawn point</returns>
    public static LobbySpawnPoint GetSpawnPoint(PlayerControl playerControl)
    {
        return SpawnPositions[playerControl.PlayerId % SpawnPositions.Count];
    }

    public struct LobbySpawnPoint
    {
        public bool IsFlipped;
        public Vector2 Position;

        public bool PlaySkinAnimation => false;
        public bool PlaySpawnAnimation => false;
    }
}