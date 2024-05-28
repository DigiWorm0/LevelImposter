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

        protected override void OnPlayerEnter(PlayerControl player)
        {
            if (player.AmOwner)
            {
                FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
                if (camera != null)
                {
                    camera.shakeAmount = _shakeAmount;
                    camera.shakePeriod = _shakePeriod;
                }
            }
        }

        protected override void OnPlayerExit(PlayerControl player)
        {
            if (player.AmOwner)
            {
                FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
                if (camera != null)
                {
                    camera.shakeAmount = 0.0f;
                    camera.shakePeriod = 0.0f;
                }
            }
        }
    }
}
