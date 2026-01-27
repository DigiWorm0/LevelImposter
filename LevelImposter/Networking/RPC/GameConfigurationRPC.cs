using System;
using Hazel;
using LevelImposter.Core;
using LevelImposter.Lobby;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Networking;

/// <summary>
/// Represents a serialized version of <see cref="GameConfiguration"/> containing map IDs and settings.
/// </summary>
public struct SerializedGameConfiguration
{
    public Guid MapID;
    public Guid LobbyMapID;
    public bool HideMapName;

    public new string ToString()
    {
        return $"MapID=[{MapID}]\nLobbyMapID=[{LobbyMapID}]\nHideMapName={HideMapName}";
    }
}

[RegisterCustomRpc((uint)LIRpc.SyncGameConfiguration)]
public class GameConfigurationRPC(LevelImposter plugin, uint id) : 
PlayerCustomRpc<LevelImposter, SerializedGameConfiguration>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None; // <-- MapSync is for other clients only
    
    public override void Write(MessageWriter writer, SerializedGameConfiguration data)
    {
        writer.Write(data.MapID.ToByteArray());
        writer.Write(data.LobbyMapID.ToByteArray());
        writer.Write(data.HideMapName);
    }

    public override SerializedGameConfiguration Read(MessageReader reader)
    {
        var gameMapID = ReadGUID(reader);
        var lobbyMapID = ReadGUID(reader);
        var isMapNameHidden = reader.ReadBoolean();
        
        return new SerializedGameConfiguration
        {
            MapID = gameMapID,
            LobbyMapID = lobbyMapID,
            HideMapName = isMapNameHidden
        };
    }

    public override void Handle(PlayerControl innerNetObject, SerializedGameConfiguration data)
    {
        LILogger.Info($"[RPC] Received game configuration: \n{data.ToString()}");
        GameConfigurationSync.OnGameConfigurationRPC(data);
    }

    /// <summary>
    /// Reads a GUID from a byte array written in a message.
    /// </summary>
    /// <param name="reader">The message reader.</param>
    /// <returns>>The GUID read from the reader.</returns>
    private Guid ReadGUID(MessageReader reader)
    {
        var guidBytes = new byte[16];
        for (var i = 0; i < guidBytes.Length; i++)
            guidBytes[i] = reader.ReadByte();
        return new Guid(guidBytes);
    }
}