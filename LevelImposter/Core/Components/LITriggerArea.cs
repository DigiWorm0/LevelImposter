using System;
using System.Collections.Generic;
using System.Text;
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

        public void OnTriggerEnter2D(Collider2D collider)
        {
            if (MapUtils.IsLocalPlayer(collider.gameObject))
                LITriggerable.Trigger(transform.gameObject, "onEnter", PlayerControl.LocalPlayer);
        }

        public void OnTriggerExit2D(Collider2D collider)
        {
            if (MapUtils.IsLocalPlayer(collider.gameObject))
                LITriggerable.Trigger(transform.gameObject, "onExit", PlayerControl.LocalPlayer);
        }
    }
}
