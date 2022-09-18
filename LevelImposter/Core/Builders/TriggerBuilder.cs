using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class TriggerBuilder : Builder
    {
        private List<LITriggerable> triggerDB = new List<LITriggerable>();

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.properties.triggers == null)
                return;

            LITrigger[] triggers = elem.properties.triggers;
            foreach (LITrigger trigger in triggers)
            {
                LITriggerable triggerComp = obj.AddComponent<LITriggerable>();
                triggerComp.id = trigger.id;
                triggerComp.elemID = elem.id;
                triggerComp.targetElemID = trigger.elemID;
                triggerComp.targetTriggerID = trigger.triggerID;

                triggerDB.Add(triggerComp);
            }
        }

        public void PostBuild()
        {
            foreach (LITriggerable trigger in triggerDB)
            {
                if (trigger.targetElemID != null && trigger.targetTriggerID != null)
                {
                    trigger.targetTrigger = Array.Find(triggerDB.ToArray(), (LITriggerable t) => t.id == trigger.targetTriggerID && t.elemID == trigger.targetElemID);
                }
            }
            triggerDB.Clear();
        }
    }
}
