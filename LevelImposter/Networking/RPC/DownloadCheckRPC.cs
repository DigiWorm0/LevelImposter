using System;
using Hazel;
using InnerNet;
using LevelImposter.Core;
using LevelImposter.Lobby;
using LevelImposter.Shop;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Networking;

[RegisterCustomRpc((uint)LIRpc.DownloadCheck)]
public class DownloadCheckRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, bool>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, bool isDownloaded)
    {
        writer.Write(isDownloaded);
    }

    public override bool Read(MessageReader reader)
    {
        return reader.ReadBoolean();
    }

    public override void Handle(PlayerControl player, bool isDownloaded)
    {
        // Log
        LILogger.Info($"[RPC] {player.name} {(isDownloaded ? "has downloaded" : "is downloading")} the map");
        
        // If the start countdown is running, reset it
        if (DestroyableSingleton<GameStartManager>.InstanceExists)
            DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
        
        // Add or Remove from Download Manager
        if (isDownloaded)
            DownloadManager.RemovePlayer(player);
        else
            DownloadManager.AddPlayer(player);
    }
}