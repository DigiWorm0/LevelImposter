using Hazel;
using LevelImposter.Core;
using LevelImposter.Lobby;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Networking;

[RegisterCustomRpc((uint)LIRpc.ReadyToStart)]
public class ReadyToStartRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, bool>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, bool isReady)
    {
        writer.Write(isReady);
    }

    public override bool Read(MessageReader reader)
    {
        return reader.ReadBoolean();
    }

    public override void Handle(PlayerControl player, bool isReady)
    {
        // Log
        LILogger.Info($"[RPC] {player.name} {(isReady ? "is ready" : "is not ready")}");
        
        // If the start countdown is running, reset it
        if (DestroyableSingleton<GameStartManager>.InstanceExists)
            DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
        
        // Add or Remove from PlayersReadyCounter
        if (isReady)
            PlayersReadyCounter.MarkPlayerReady(player);
        else
            PlayersReadyCounter.MarkPlayerNotReady(player);
    }
}