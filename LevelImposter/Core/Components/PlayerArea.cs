using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that tracks the players that are inside it's range
/// </summary>
public class PlayerArea(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    [HideFromIl2Cpp] public List<byte>? CurrentPlayersIDs { get; private set; } = new();

    public bool IsLocalPlayerInside { get; private set; }

    public void OnDestroy()
    {
        CurrentPlayersIDs = null;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        var player = collider.GetComponent<PlayerControl>();
        if (player == null)
            return;

        CurrentPlayersIDs?.Add(player.PlayerId);
        if (player.AmOwner)
            IsLocalPlayerInside = true;

        if (enabled)
            OnPlayerEnter(player);
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        var player = collider.GetComponent<PlayerControl>();
        if (player == null)
            return;

        CurrentPlayersIDs?.RemoveAll(id => id == player.PlayerId);
        if (player.AmOwner)
            IsLocalPlayerInside = false;

        if (enabled)
            OnPlayerExit(player);
    }

    /// <summary>
    ///     Gets a player by it's ID
    /// </summary>
    /// <param name="playerID">Player ID to search</param>
    /// <returns>The cooresponding PlayerControl or null if it can't be found</returns>
    public PlayerControl? GetPlayer(byte playerID)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.PlayerId == playerID)
                return player;
        return null;
    }

    /// <summary>
    ///     Called when a player enters the collider
    /// </summary>
    /// <param name="player">Player that entered the collider</param>
    public virtual void OnPlayerEnter(PlayerControl player)
    {
    }

    /// <summary>
    ///     Called when a player exits the collider
    /// </summary>
    /// <param name="player">Player that exited the collider</param>
    public virtual void OnPlayerExit(PlayerControl player)
    {
    }
}