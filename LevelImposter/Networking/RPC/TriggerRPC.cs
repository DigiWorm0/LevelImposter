using System;
using Hazel;
using InnerNet;
using LevelImposter.Core;
using LevelImposter.Shop;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Networking;

public struct RPCTriggerPacket
{
    public string ElemIDString;
    public string TriggerID;
}

[RegisterCustomRpc((uint)LIRpc.FireTrigger)]
public class TriggerRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, RPCTriggerPacket>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, RPCTriggerPacket data)
    {
        writer.Write(data.ElemIDString);
        writer.Write(data.TriggerID);
    }

    public override RPCTriggerPacket Read(MessageReader reader)
    {
        var elemIDString = reader.ReadString();
        var triggerID = reader.ReadString();
        
        return new RPCTriggerPacket
        {
            ElemIDString = elemIDString,
            TriggerID = triggerID
        };
    }

    public override void Handle(PlayerControl orginPlayer, RPCTriggerPacket data)
    {
        // Log
        if (TriggerSystem.EnableLogging)
            LILogger.Msg($"[RPC] {data.ElemIDString} >>> {data.TriggerID} ({orginPlayer.name})");

        // Parse ID
        if (!Guid.TryParse(data.ElemIDString, out var elemID))
        {
            LILogger.Warn("RPC triggered element ID is invalid.");
            return;
        }

        // Find cooresponding object
        var gameObject = LIBaseShip.Instance?.MapObjectDB.GetObject(elemID);
        if (gameObject == null)
        {
            LILogger.Warn($"RPC object with ID {elemID} is missing");
            return;
        }

        // Create & Fire Trigger
        TriggerSignal signal = new(gameObject, data.TriggerID, orginPlayer);
        TriggerSystem.GetInstance().FireTrigger(signal);
    }
}