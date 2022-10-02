using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class TriggerBuilder : IElemBuilder
    {
        private List<LITriggerable> _triggerDB = new List<LITriggerable>();

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.properties.triggers == null)
                return;

            LITrigger[] triggers = elem.properties.triggers;
            foreach (LITrigger trigger in triggers)
            {
                LITriggerable triggerComp = obj.AddComponent<LITriggerable>();
                triggerComp.ID = trigger.id;
                triggerComp.ElemID = elem.id;
                triggerComp.TargetElemID = trigger.elemID;
                triggerComp.TargetTriggerID = trigger.triggerID;
                triggerComp.CurrentElem = elem;

                _triggerDB.Add(triggerComp);
            }
        }

        public void PostBuild()
        {
            foreach (LITriggerable trigger in _triggerDB)
            {
                if (trigger.TargetElemID != null && trigger.TargetTriggerID != null)
                {
                    trigger.targetTrigger = Array.Find(_triggerDB.ToArray(), (LITriggerable t) => t.ID == trigger.TargetTriggerID && t.ElemID == trigger.TargetElemID);
                }
            }
            _triggerDB.Clear();
        }
    }
}
