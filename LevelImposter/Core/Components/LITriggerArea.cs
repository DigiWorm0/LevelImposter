using System;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that fires a trigger when the player enters/exits it's range
    /// </summary>
    public class LITriggerArea : PlayerArea
    {
        public LITriggerArea(IntPtr intPtr) : base(intPtr)
        {
        }

        private const string ENTER_TRIGGGER_ID = "onEnter";
        private const string EXIT_TRIGGER_ID = "onExit";

        private bool _isClientSide = false;

        /// <summary>
        /// Sets whether or not the Trigger Area is client sided
        /// </summary>
        /// <param name="isClientSide">TRUE if the trigger is client sided</param>
        public void SetClientSide(bool isClientSide)
        {
            _isClientSide = isClientSide;
        }

        protected override void OnPlayerEnter(PlayerControl player)
        {
            bool triggerServerSided = CurrentPlayersIDs?.Count <= 1 && !_isClientSide;
            bool triggerClientSided = player.AmOwner && _isClientSide;
            if (triggerClientSided || triggerServerSided)
                LITriggerable.Trigger(transform.gameObject, ENTER_TRIGGGER_ID, null);
        }

        protected override void OnPlayerExit(PlayerControl player)
        {
            bool triggerServerSided = CurrentPlayersIDs?.Count <= 0 && !_isClientSide;
            bool triggerClientSided = player.AmOwner && _isClientSide;
            if (triggerClientSided || triggerServerSided)
                LITriggerable.Trigger(transform.gameObject, EXIT_TRIGGER_ID, null);
        }
    }
}
