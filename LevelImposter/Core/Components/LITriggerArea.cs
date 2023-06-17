using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that fires a trigger when the player enters/exits it's range
    /// </summary>
    public class LITriggerArea : MonoBehaviour
    {
        public LITriggerArea(IntPtr intPtr) : base(intPtr)
        {
        }

        private const string ENTER_TRIGGGER_ID = "onEnter";
        private const string EXIT_TRIGGER_ID = "onExit";

        private List<byte>? _currentPlayersIDs = new();
        private bool _isClientSide = false;

        /// <summary>
        /// Sets whether or not the Trigger Area is client sided
        /// </summary>
        /// <param name="isClientSide">TRUE if the trigger is client sided</param>
        public void SetClientSide(bool isClientSide)
        {
            _isClientSide = isClientSide;
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            PlayerControl? player = collider.GetComponent<PlayerControl>();
            if (player == null)
                return;

            bool triggerServerSided = _currentPlayersIDs?.Count <= 0 && !_isClientSide;
            bool triggerClientSided = MapUtils.IsLocalPlayer(collider.gameObject) && _isClientSide;
            if (triggerClientSided || triggerServerSided)
                LITriggerable.Trigger(transform.gameObject, ENTER_TRIGGGER_ID, null);

            if (_currentPlayersIDs?.Contains(player.PlayerId) ?? false)
                _currentPlayersIDs?.Add(player.PlayerId);
        }
        public void OnTriggerExit2D(Collider2D collider)
        {
            PlayerControl? player = collider.GetComponent<PlayerControl>();
            if (player == null)
                return;

            _currentPlayersIDs?.RemoveAll(id => id == player.PlayerId);

            bool triggerServer = _currentPlayersIDs?.Count <= 0 && !_isClientSide;
            bool triggerClient = MapUtils.IsLocalPlayer(collider.gameObject) && _isClientSide;
            if (triggerClient || triggerServer)
                LITriggerable.Trigger(transform.gameObject, EXIT_TRIGGER_ID, null);
        }
        public void OnDestroy()
        {
            _currentPlayersIDs = null;
        }
    }
}
