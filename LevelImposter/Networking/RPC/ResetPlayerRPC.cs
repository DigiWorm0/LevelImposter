using Hazel;
using LevelImposter.Core;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

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
        playerPhysics.transform.position = GetRespawnPosition();

        // Reset animations
        playerPhysics.Animations.PlayIdleAnimation();

        // Handle if I'm the Player
        if (!playerPhysics.AmOwner)
            return;
        if (ShipStatus.Instance != null) // <-- Prevents NullReferenceException on ExitAllVents
            playerPhysics.ExitAllVents();
        LILogger.Notify("You've been reset to spawn", false);
    }

    private static Vector2 GetRespawnPosition()
    {
        // Lobby
        if (LobbyBehaviour.Instance != null)
            return LobbyBehaviour.Instance.SpawnPositions[0];

        // Game
        if (ShipStatus.Instance != null)
            return ShipStatus.Instance.InitialSpawnCenter;

        // Fallback
        return Vector2.zero;
    }
}