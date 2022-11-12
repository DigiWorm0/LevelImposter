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

        public void OnTriggerEnter2D(Collider2D collider)
        {
            if (MapUtils.IsLocalPlayer(collider.gameObject))
                MapUtils.FireTrigger(transform.gameObject, "onEnter", collider.gameObject);
        }

        public void OnTriggerExit2D(Collider2D collider)
        {
            if (MapUtils.IsLocalPlayer(collider.gameObject))
                MapUtils.FireTrigger(transform.gameObject, "onExit", collider.gameObject);
        }
    }
}
