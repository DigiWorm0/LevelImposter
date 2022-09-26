using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class TriggerAreaBuilder : IElemBuilder
    {

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-triggerarea")
                return;

            // Colliders
            Collider2D[] colliders = obj.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                collider.isTrigger = true;
            }

            // TriggerArea
            obj.AddComponent<LITriggerArea>();
        }

        public void PostBuild()
        {
        }
    }
}
