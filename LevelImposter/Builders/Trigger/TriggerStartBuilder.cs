using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class TriggerStartBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-triggerstart")
                return;

            // TODO: Add onHideAndSeekStart & onClassicStart
            LITriggerSpawnable trigger = obj.AddComponent<LITriggerSpawnable>();
            trigger.SetTrigger(obj, "onStart");
            obj.SetActive(true);
        }

        public void PostBuild() { }
    }
}
