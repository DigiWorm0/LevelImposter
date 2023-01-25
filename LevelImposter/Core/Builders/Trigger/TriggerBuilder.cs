using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class TriggerBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.properties.triggers == null)
                return;

            LITrigger[] triggers = elem.properties.triggers;
            foreach (LITrigger trigger in triggers)
            {
                LITriggerable triggerComp = obj.AddComponent<LITriggerable>();
                triggerComp.SetTrigger(
                    elem,
                    trigger.id,
                    trigger.elemID,
                    trigger.triggerID
                );

            }
        }

        public void PostBuild() { }
    }
}
