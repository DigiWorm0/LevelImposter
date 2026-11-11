using System;
using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Patches;
using UnityEngine;

namespace LevelImposter.Build.Builders.Lobby;

internal static class LobbySpawnBuilder
{
    private static readonly List<LobbySpawnPoint> SpawnPositions = [];

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        SpawnPositions.Clear();
    }

    [ElementBuilder(
        Target = MapTarget.Lobby,
        ElementTypes = ["util-lobbyspawn"]
    )]
    public static void Build(LobbyBehaviour lobbyBehaviour, GameObject gameObject)
    {
        // Add to local list
        SpawnPositions.Add(new LobbySpawnPoint
        {
            IsFlipped = gameObject.transform.localScale.x < 0,
            Position = gameObject.transform.position
        });

        // Add to Lobby Behavior list
        lobbyBehaviour.SpawnPositions = lobbyBehaviour.SpawnPositions.Add(gameObject.transform.position);
    }

    [MapBuilder(
        Target = MapTarget.Lobby,
        Priority = Priority.LAST
    )]
    public static void OnPostBuild(LobbyBehaviour lobbyBehaviour)
    {
        // Check if no spawn positions were added
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
        if (SpawnPositions.Count == 0)
            throw new Exception("Lobby spawn points have not yet been loaded");

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