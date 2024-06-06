using Reactor.Networking.Attributes;
using System;
using System.Linq;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that kills all players that enter it's range
    /// </summary>
    public class LIDeathArea : PlayerArea
    {
        public LIDeathArea(IntPtr intPtr) : base(intPtr)
        {
        }

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
            byte[] playerIDs = CurrentPlayersIDs.ToArray(); // <-- Copy to avoid mutation during iteration
            foreach (byte playerID in playerIDs)
            {
                // Get Player by ID
                PlayerControl? player = GetPlayer(playerID);
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
            DeadBody deadBody = Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.enabled = false;
            deadBody.ParentId = player.PlayerId;

            // Set Colors
            foreach (SpriteRenderer renderer in deadBody.bodyRenderers)
                player.SetPlayerMaterialColors(renderer);
            player.SetPlayerMaterialColors(deadBody.bloodSplatter);

            // Set Offset
            Vector3 bodyOffset = player.KillAnimations.First().BodyOffset;
            Vector3 bodyPosition = player.transform.position + bodyOffset;
            bodyPosition.z = bodyPosition.y / 1000f;
            deadBody.transform.position = bodyPosition;

            // Enable Dead Body
            deadBody.enabled = true;
        }

        private static void PlayKillSound()
        {
            AudioClip killSFX = PlayerControl.LocalPlayer.KillSfx;
            SoundManager.Instance.PlaySound(killSFX, false, 0.8f);
        }
    }
}
