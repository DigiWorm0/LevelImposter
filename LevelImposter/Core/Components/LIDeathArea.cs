using System;
using System.Linq;
using LevelImposter.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LevelImposter.Core;

/// <summary>
///     Object that kills all players that enter its range
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
            Rpc<DeathAreaRPC>.Instance.Send(player, _createDeadBody, true);
        }
    }

    public static void CreateDeadBody(PlayerControl player)
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

    public static void PlayKillSFX()
    {
        var killSFX = PlayerControl.LocalPlayer.KillSfx;
        SoundManager.Instance.PlaySound(killSFX, false, 0.8f);
    }
}