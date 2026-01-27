using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.Lobby;
using UnityEngine;

namespace LevelImposter.Builders;

public class LobbySpawnBuilder : IElemBuilder
{
    public void OnPreBuild()
    {
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-lobbyspawn")
            return;
        
        var lobbyBehaviour = LILobbyBehaviour.GetLobbyBehaviour();
        lobbyBehaviour.SpawnPositions = MapUtils.AddToArr(lobbyBehaviour.SpawnPositions, obj.transform.position);
    }

    public void OnPostBuild()
    {
        // Check if no spawn positions were added
        var lobbyBehaviour = LILobbyBehaviour.GetLobbyBehaviour();
        if (lobbyBehaviour.SpawnPositions.Length == 0)
            lobbyBehaviour.SpawnPositions = new[] { Vector2.zero };
        
        // Replay the spawn animations for all players
        foreach (var playerControl in PlayerControl.AllPlayerControls)
            playerControl.StartCoroutine(
                playerControl.MyPhysics.CoSpawnPlayer(lobbyBehaviour));
    }
}