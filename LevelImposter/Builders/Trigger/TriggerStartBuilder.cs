using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

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