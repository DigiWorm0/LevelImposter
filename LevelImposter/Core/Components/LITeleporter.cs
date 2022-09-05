using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class LITeleporter : MonoBehaviour
    {
        public LIElement elem = null;
        public LITeleporter target = null;

        public LITeleporter(IntPtr intPtr) : base(intPtr)
        {
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            PlayerControl player = collider.GetComponent<PlayerControl>();
            if (player != null && target != null)
            {
                // Offset
                Vector3 offset = transform.position - target.transform.position;
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
            }
        }
    }
}
