using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class LITriggerArea : MonoBehaviour
    {
        public LITriggerArea(IntPtr intPtr) : base(intPtr)
        {
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (MapUtils.IsLocalPlayer(collider.gameObject))
                MapUtils.FireTrigger(gameObject, "onEnter", collider.gameObject);
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (MapUtils.IsLocalPlayer(collider.gameObject))
                MapUtils.FireTrigger(gameObject, "onExit", collider.gameObject);
        }
    }
}
