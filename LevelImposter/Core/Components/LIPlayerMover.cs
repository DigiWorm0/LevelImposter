using System;
using System.Collections.Generic;
using LevelImposter.Networking.RPC;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace LevelImposter.Core.Components;

/// <summary>
///     Object that fires a trigger when the player enters/exits it's range
/// </summary>
public class LIPlayerMover(IntPtr intPtr) : PlayerArea(intPtr)
{
    public static readonly Dictionary<uint, Transform> AllObjects = new();
    private uint _objectID;

    public new void OnDestroy()
    {
        base.OnDestroy();
        AllObjects.Remove(_objectID);
    }

    public void SetObjectID(uint objectID)
    {
        _objectID = objectID;
        AllObjects.Add(_objectID, transform);
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
        SyncLocalPlayer(player);
    }

    private void SyncLocalPlayer(PlayerControl playerControl, uint targetID = 0)
    {
        var playerTransform = playerControl.transform;

        Rpc<PlayerMoverRPC>.Instance.Send(playerControl, new RPCPlayerMoverPacket
        {
            ParentTransformID = targetID,
            X = playerTransform.localPosition.x,
            Y = playerTransform.localPosition.y,
            Rotation = playerTransform.localRotation.eulerAngles.z,
            ScaleX = playerTransform.localScale.x,
            ScaleY = playerTransform.localScale.y
        }, true);
    }
}