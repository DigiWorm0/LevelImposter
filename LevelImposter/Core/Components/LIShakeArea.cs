using System;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that shakes the screen when players enter it's range
    /// </summary>
    public class LIShakeArea : PlayerArea
    {
        public LIShakeArea(IntPtr intPtr) : base(intPtr)
        {
        }

        private float _shakeAmount = 0.03f;
        private float _shakePeriod = 400.0f;

        public void SetParameters(float shakeAmount, float shakePeriod)
        {
            _shakeAmount = shakeAmount;
            _shakePeriod = shakePeriod;
        }

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
            SetShakeEnabled(enabled && IsLocalPlayerInside);
        }

        private void SetShakeEnabled(bool enabled)
        {
            FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
            if (camera != null)
            {
                camera.shakeAmount = enabled ? _shakeAmount : 0.0f;
                camera.shakePeriod = enabled ? _shakePeriod : 0.0f;
            }
        }

        protected override void OnPlayerEnter(PlayerControl player)
        {
            LILogger.Info("Player Entered");
            if (player.AmOwner)
                SetShakeEnabled(true);
        }

        protected override void OnPlayerExit(PlayerControl player)
        {
            if (player.AmOwner)
                SetShakeEnabled(false);
        }
    }
}
