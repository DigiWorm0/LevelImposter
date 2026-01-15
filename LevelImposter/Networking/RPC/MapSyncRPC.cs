using System;
using Hazel;
using LevelImposter.Core;
using LevelImposter.Shop;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Networking;

public struct MapState
{
    public Guid MapID;
    public int RandomizerSeed;
    public bool IsMapNameHidden;
}

[RegisterCustomRpc((uint)LIRpc.SyncMapID)]
public class MapSyncRPC(LevelImposter plugin, uint id) : PlayerCustomRpc<LevelImposter, MapState>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None; // <-- MapSync is for other clients only
    
    public override void Write(MessageWriter writer, MapState data)
    {
        var mapIDBytes = data.MapID.ToByteArray();
        foreach (var mapIDByte in mapIDBytes)
            writer.Write(mapIDByte);
        
        writer.Write(data.RandomizerSeed);
        writer.Write(data.IsMapNameHidden);
    }

    public override MapState Read(MessageReader reader)
    {
        var mapIDBytes = new byte[16];
        for (var i = 0; i < mapIDBytes.Length; i++)
            mapIDBytes[i] = reader.ReadByte();
        
        var mapID = new Guid(mapIDBytes);
        var randomizerSeed = reader.ReadInt32();
        var isMapNameHidden = reader.ReadBoolean();
        
        return new MapState
        {
            MapID = mapID,
            RandomizerSeed = randomizerSeed,
            IsMapNameHidden = isMapNameHidden
        };
    }

    public override void Handle(PlayerControl innerNetObject, MapState data)
    {
        LILogger.Info($"[RPC] Received map ID [{data.MapID.ToString()}] (Hidden={data.IsMapNameHidden})");
        MapSync.OnRPCSyncMapID(data.MapID, data.IsMapNameHidden);
        RandomizerSync.SetRandomSeed(data.RandomizerSeed);
    }
}