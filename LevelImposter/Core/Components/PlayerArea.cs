using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that tracks the players that are inside it's range
    /// </summary>
    public class PlayerArea : MonoBehaviour
    {
        public PlayerArea(IntPtr intPtr) : base(intPtr)
        {
        }

        private List<byte>? _currentPlayersIDs = new();
        private bool _isLocalPlayerInside = false;

        [HideFromIl2Cpp]
        protected List<byte>? CurrentPlayersIDs => _currentPlayersIDs;
        protected bool IsLocalPlayerInside => _isLocalPlayerInside;

        /// <summary>
        /// Gets a player by it's ID
        /// </summary>
        /// <param name="playerID">Player ID to search</param>
        /// <returns>The cooresponding PlayerControl or null if it can't be found</returns>
        protected PlayerControl? GetPlayer(byte playerID)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == playerID)
                    return player;
            return null;
        }

        /// <summary>
        /// Called when a player enters the collider
        /// </summary>
        /// <param name="player">Player that entered the collider</param>
        virtual protected void OnPlayerEnter(PlayerControl player)
        {
        }

        /// <summary>
        /// Called when a player exits the collider
        /// </summary>
        /// <param name="player">Player that exited the collider</param>
        virtual protected void OnPlayerExit(PlayerControl player)
        {
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            PlayerControl? player = collider.GetComponent<PlayerControl>();
            if (player == null)
                return;

            _currentPlayersIDs?.Add(player.PlayerId);
            if (player.AmOwner)
                _isLocalPlayerInside = true;

            if (enabled)
                OnPlayerEnter(player);
        }

        public void OnTriggerExit2D(Collider2D collider)
        {
            PlayerControl? player = collider.GetComponent<PlayerControl>();
            if (player == null)
                return;

            _currentPlayersIDs?.RemoveAll(id => id == player.PlayerId);
            if (player.AmOwner)
                _isLocalPlayerInside = false;

            if (enabled)
                OnPlayerExit(player);
        }
        public void OnDestroy()
        {
            _currentPlayersIDs = null;
        }
    }
}
