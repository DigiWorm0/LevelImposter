using System;
using System.Linq;
using Reactor.Networking.Attributes;

namespace LevelImposter.Core;

/// <summary>
///     Object that kills all players that enter it's range
/// </summary>
public class LIDeathArea(IntPtr intPtr) : PlayerArea(intPtr)
{
    private bool _createDeadBody = true;

    public void SetCreateDeadBody(bool value)
    {
        _createDeadBody = value;
    }

    public void KillAllPlayers()
    {
        if (CurrentPlayersIDs == null)
            return;

        // Only the host can handle kill triggers?
        if (!AmongUsClient.Instance.AmHost)
            return;

        // Iterate over all players in the area
        var playerIDs = CurrentPlayersIDs.ToArray(); // <-- Copy to avoid mutation during iteration
        foreach (var playerID in playerIDs)
        {
            // Get Player by ID
            var player = GetPlayer(playerID);
            if (player == null)
                continue;

            // Fire RPC to kill player
            RPCTriggerDeath(player, _createDeadBody);
        }
    }

    [MethodRpc((uint)LIRpc.KillPlayer)]
    public static void RPCTriggerDeath(PlayerControl player, bool createDeadBody)
    {
        if (player == null || player.Data.IsDead)
            return;
        LILogger.Info($"[RPC] Trigger killing {player.name}");

        // Kill Player
        player.Die(DeathReason.Kill, false);

        // Play Kill Sound (if I'm the Player)
        if (player.AmOwner)
            PlayKillSound();

        // Create Dead Body
        if (createDeadBody)
            CreateDeadBody(player);
    }

    private static void CreateDeadBody(PlayerControl player)
    {
        // Create/Disable Dead Body
        var deadBodyPrefab = GameManager.Instance.deadBodyPrefab.First();
        var deadBody = Instantiate(deadBodyPrefab);
        deadBody.enabled = false;
        deadBody.ParentId = player.PlayerId;

        // Set Colors
        foreach (var renderer in deadBody.bodyRenderers)
            player.SetPlayerMaterialColors(renderer);
        player.SetPlayerMaterialColors(deadBody.bloodSplatter);

        // Set Offset
        var bodyOffset = player.KillAnimations.First().BodyOffset;
        var bodyPosition = player.transform.position + bodyOffset;
        bodyPosition.z = bodyPosition.y / 1000f;
        deadBody.transform.position = bodyPosition;

        // Enable Dead Body
        deadBody.enabled = true;
    }

    private static void PlayKillSound()
    {
        var killSFX = PlayerControl.LocalPlayer.KillSfx;
        SoundManager.Instance.PlaySound(killSFX, false, 0.8f);
    }
}