using System;
using Hazel;
using InnerNet;
using LevelImposter.Core;
using LevelImposter.Shop;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Networking;

[RegisterCustomRpc((uint)LIRpc.KillPlayer)]
public class DeathAreaRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, bool>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, bool createDeadBody)
    {
        writer.Write(createDeadBody);
    }

    public override bool Read(MessageReader reader)
    {
        return reader.ReadBoolean();
    }

    public override void Handle(PlayerControl player, bool createDeadBody)
    {
        if (player == null || player.Data.IsDead)
            return;
        
        // Log
        LILogger.Info($"[RPC] Trigger killing {player.name}");

        // Kill Player
        player.Die(DeathReason.Kill, false);

        // Play Kill Sound (if I'm the Player)
        if (player.AmOwner)
            LIDeathArea.PlayKillSFX();

        // Create Dead Body
        if (createDeadBody)
            LIDeathArea.CreateDeadBody(player);
    }
}