using System;
using Hazel;
using InnerNet;
using LevelImposter.Core;
using LevelImposter.Shop;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace LevelImposter.Networking;

public struct RPCPlayerMoverPacket
{
    public uint ParentTransformID;
    public float X;
    public float Y;
    public float Rotation;
    public float ScaleX;
    public float ScaleY;
}

[RegisterCustomRpc((uint)LIRpc.SyncPlayerMover)]
public class PlayerMoverRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, RPCPlayerMoverPacket>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, RPCPlayerMoverPacket data)
    {
        writer.Write(data.ParentTransformID);
        writer.Write(data.X);
        writer.Write(data.Y);
        writer.Write(data.Rotation);
        writer.Write(data.ScaleX);
        writer.Write(data.ScaleY);
    }

    public override RPCPlayerMoverPacket Read(MessageReader reader)
    {
        var objectID = reader.ReadUInt32();
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var rotation = reader.ReadSingle();
        var scaleX = reader.ReadSingle();
        var scaleY = reader.ReadSingle();
        
        return new RPCPlayerMoverPacket
        {
            ParentTransformID = objectID,
            X = x,
            Y = y,
            Rotation = rotation,
            ScaleX = scaleX,
            ScaleY = scaleY
        };
    }

    public override void Handle(PlayerControl playerControl, RPCPlayerMoverPacket data)
    {
        // Log
        LILogger.Debug($"[RPC] {playerControl.name} syncing to player mover ({data.ParentTransformID})");
        
        // Find Player Mover
        if (LIPlayerMover.AllObjects.TryGetValue(data.ParentTransformID, out var parentTransform))
            // Set parent to player mover
            playerControl.transform.SetParent(parentTransform);
        else
            // No parent found, default to ship status
            playerControl.transform.SetParent(ShipStatus.Instance.transform);
        
        // Apply transform
        playerControl.transform.localPosition = new Vector3(data.X, data.Y, 0);
        playerControl.transform.localRotation = Quaternion.Euler(0, 0, data.Rotation);
        playerControl.transform.localScale = new Vector3(data.ScaleX, data.ScaleY, 1);
    }
}