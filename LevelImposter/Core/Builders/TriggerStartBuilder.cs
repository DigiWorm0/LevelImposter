using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class TriggerStartBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-triggerstart")
                return;

            LITriggerSpawnable trigger = obj.AddComponent<LITriggerSpawnable>();
            trigger.SetTrigger(obj, "onStart", true);
            obj.SetActive(true);
        }

        public void PostBuild() { }
    }
}
