using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class TriggerAreaBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-triggerarea")
                return;

            // Colliders
            Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
                collider.isTrigger = true;

            LITriggerArea triggerArea = obj.AddComponent<LITriggerArea>();
            triggerArea.SetClientSide(elem.properties.triggerClientSide != false);
        }

        public void PostBuild() { }
    }
}
