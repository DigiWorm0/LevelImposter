using Il2CppInterop.Runtime.Attributes;
using Reactor.Networking.Attributes;
using System;
using System.Collections.Generic;
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

        private static List<LITeleporter> _teleList = new();

        private LIElement? _elem = null;
        private LITeleporter? _target = null;
        private bool _preserveOffset = true;
        private bool _enabled = true;

        [HideFromIl2Cpp]
        public LIElement? CurrentElem => _elem;
        public LITeleporter? CurrentTarget => _target;


        /// <summary>
        /// Sets the Teleporter's LIElement source
        /// </summary>
        /// <param name="elem">Element to read properties from</param>
        [HideFromIl2Cpp]
        public void SetElement(LIElement elem)
        {
            _elem = elem;
            _preserveOffset = elem.properties.preserveOffset ?? true;
            _enabled = elem.properties.isEnabled ?? true;
        }

        /// <summary>
        /// RPC that is ran when the player is teleported
        /// </summary>
        /// <param name="player">PlayerControl that is teleported</param>
        /// <param name="x">Global X position to teleport to</param>
        /// <param name="y">Global Y position to teleport to</param>
        [MethodRpc((uint)LIRpc.TeleportPlayer)]
        public static void RPCTeleport(PlayerControl player, float x, float y)
        {
            LILogger.Info($"Teleported {player.name} to ({x},{y})");
            player.NetTransform.SnapTo(player.transform.position);
        }

        public void Awake()
        {
            _teleList.Add(this);
        }
        public void Start()
        {
            if (_elem == null)
                return;
            foreach (var teleporter in _teleList)
            {
                Guid? targetID = _elem.properties.teleporter;
                if (targetID != null)
                {
                    _target = _teleList.Find((tele) => tele.CurrentElem?.id == targetID);
                }
            }
        }
        public void OnDestroy()
        {
            _teleList.Clear();
            _elem = null;
            _target = null;
        }
        public void OnTriggerEnter2D(Collider2D collider)
        {
            PlayerControl player = collider.GetComponent<PlayerControl>();
            if (player == null)
                return;
            if (_elem == null || _target == null || !_enabled)
                return;
            if (!MapUtils.IsLocalPlayer(player.gameObject))
                return;
            if (collider.TryCast<CircleCollider2D>() == null) // Disable BoxCollider2D
                return;

            // Offset
            Vector3 offset;
            if (_preserveOffset)
                offset = transform.position - _target.transform.position;
            else
                offset = player.transform.position - _target.transform.position;
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
            RPCTeleport(
                player,
                player.transform.position.x,
                player.transform.position.y
            );
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public bool isEnabled()
        {
            return _enabled;
        }
    }
}
