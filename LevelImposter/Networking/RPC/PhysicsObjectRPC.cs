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

public struct RPCPhysicsObjectPacket
{
    public uint ObjectID;
    public float X;
    public float Y;
    public float Rotation;
    public float VelocityX;
    public float VelocityY;
    public float AngularVelocity;
}

[RegisterCustomRpc((uint)LIRpc.SyncPhysicsObject)]
public class PhysicsObjectRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, RPCPhysicsObjectPacket>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, RPCPhysicsObjectPacket data)
    {
        writer.Write(data.ObjectID);
        writer.Write(data.X);
        writer.Write(data.Y);
        writer.Write(data.Rotation);
        writer.Write(data.VelocityX);
        writer.Write(data.VelocityY);
        writer.Write(data.AngularVelocity);
    }

    public override RPCPhysicsObjectPacket Read(MessageReader reader)
    {
        var objectID = reader.ReadUInt32();
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var rotation = reader.ReadSingle();
        var velocityX = reader.ReadSingle();
        var velocityY = reader.ReadSingle();
        var angularVelocity = reader.ReadSingle();
        
        return new RPCPhysicsObjectPacket
        {
            ObjectID = objectID,
            X = x,
            Y = y,
            Rotation = rotation,
            VelocityX = velocityX,
            VelocityY = velocityY,
            AngularVelocity = angularVelocity
        };
    }

    public override void Handle(PlayerControl orginPlayer, RPCPhysicsObjectPacket data)
    {
        // Log
        LILogger.Debug($"[RPC] {orginPlayer.name} updated physics object {data.ObjectID}");

        // Get object
        if (!LIPhysicsObject.AllObjects.TryGetValue(data.ObjectID, out var obj))
            return;

        // Update position
        obj.transform.position = new Vector3(
            data.X,
            data.Y,
            obj.liObject?.Element.z ?? 0
        );
        obj.transform.position = MapUtils.ScaleZPositionByY(obj.transform.position);
        obj.transform.rotation = Quaternion.Euler(0, 0, data.Rotation);

        // Update velocity
        if (obj.rb == null)
            throw new Exception("Rigidbody2D is null");
        obj.rb.velocity = new Vector2(data.VelocityX, data.VelocityY);
        obj.rb.angularVelocity = data.AngularVelocity;
    }
}