using System;
using System.Collections.Generic;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that fires a trigger when the player enters/exits it's range
/// </summary>
public class LIPlayerMover(IntPtr intPtr) : PlayerArea(intPtr)
{
    private uint _objectID;
    private static readonly Dictionary<uint, Transform> _allObjects = new();

    public void SetObjectID(uint objectID)
    {
        _objectID = objectID;
        _allObjects.Add(_objectID, transform);
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        _allObjects.Remove(_objectID);
    }
    
    public override void OnPlayerEnter(PlayerControl player)
    {
        if (!player.AmOwner)
            return;
        
        player.transform.SetParent(transform);
        SyncLocalPlayer(player, _objectID);
    }

    public override void OnPlayerExit(PlayerControl player)
    {
        if (!player.AmOwner)
            return;
        
        player.transform.SetParent(ShipStatus.Instance.transform);
        SyncLocalPlayer(player, 0);
    }

    private void SyncLocalPlayer(PlayerControl playerControl, uint targetID = 0)
    {
        var playerTransform = playerControl.transform;
        
        RPCSyncPlayerMover(
            playerControl,
            targetID,
            playerTransform.localPosition.x,
            playerTransform.localPosition.y,
            playerTransform.localRotation.eulerAngles.z,
            playerTransform.localScale.x,
            playerTransform.localScale.y);
    }

    [MethodRpc((uint)LIRpc.SyncPlayerMover)]
    private static void RPCSyncPlayerMover(
        PlayerControl playerControl,
        uint parentTransformID,
        float x,
        float y,
        float rotation,
        float scaleX,
        float scaleY)
    {
        // Log
        LILogger.Debug($"[RPC] {playerControl.name} syncing to player mover ({parentTransformID})");
        
        // Find Player Mover
        if (_allObjects.TryGetValue(parentTransformID, out var parentTransform))
            // Set parent to player mover
            playerControl.transform.SetParent(parentTransform);
        else
            // No parent found, default to ship status
            playerControl.transform.SetParent(ShipStatus.Instance.transform);
        
        // Apply transform
        playerControl.transform.localPosition = new Vector3(x, y, 0);
        playerControl.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        playerControl.transform.localScale = new Vector3(scaleX, scaleY, 1);
    }
}