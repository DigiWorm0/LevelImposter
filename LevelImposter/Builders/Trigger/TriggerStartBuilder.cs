using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Trigger;

public class TriggerStartBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-triggerstart")
            return;

        // TODO: Add onHideAndSeekStart & onClassicStart
        var trigger = obj.AddComponent<LITriggerSpawnable>();
        trigger.SetTrigger(obj, "onStart");
        obj.SetActive(true);
    }
}