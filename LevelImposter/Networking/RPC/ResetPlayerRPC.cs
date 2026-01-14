using System;
using Hazel;
using InnerNet;
using LevelImposter.Core;
using LevelImposter.Shop;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Networking;

[RegisterCustomRpc((uint)LIRpc.ResetPlayer)]
public class ResetPlayerRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, bool>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, bool _)
    {
    }

    public override bool Read(MessageReader reader)
    {
        return true;
    }

    public override void Handle(PlayerControl playerToReset, bool _)
    {
        if (playerToReset == null)
            return;
        
        // Log
        LILogger.Info($"[RPC] Resetting {playerToReset.name} to spawn");
        
        // Reset Player Position
        var playerPhysics = playerToReset.GetComponent<PlayerPhysics>();
        var shipStatus = LIShipStatus.GetShip();
        playerPhysics.transform.position = shipStatus.InitialSpawnCenter;
        
        // Handle if I'm the Player
        if (!playerPhysics.AmOwner)
            return;
        playerPhysics.ExitAllVents();
        LILogger.Notify("You've been reset to spawn", false);
    }
}