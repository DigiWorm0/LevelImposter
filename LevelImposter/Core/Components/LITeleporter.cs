using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that teleports the player on contact
    /// </summary>
    public class LITeleporter : MonoBehaviour
    {
        public LITeleporter(IntPtr intPtr) : base(intPtr)
        {
        }

        public LIElement CurrentElem = null;
        public LITeleporter CurrentTarget = null;

        public void OnTriggerEnter2D(Collider2D collider)
        {
            PlayerControl player = collider.GetComponent<PlayerControl>();
            if (player == null)
                return;
            if (CurrentElem == null)
                return;
            if (!MapUtils.IsLocalPlayer(player.gameObject))
                return;

            // Offset
            Vector3 offset = transform.position - CurrentTarget.transform.position;
            offset.z = 0;

            // Pet
            PetBehaviour pet = player.cosmetics.currentPet;
            if (pet != null)
            {
                pet.transform.position -= offset;
            }

            // Player
            player.transform.position -= offset;

            // Camera
            Camera.main.transform.position -= offset;
            FollowerCamera followerCam = Camera.main.GetComponent<FollowerCamera>();
            followerCam.centerPosition = Camera.main.transform.position;

            // RPC
            ReactorRPC.RPCTeleportPlayer(
                player,
                player.transform.position.x,
                player.transform.position.y
            );
        }
    }
}
