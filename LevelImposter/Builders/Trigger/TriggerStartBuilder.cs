using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using UnityEngine;

namespace LevelImposter.Builders.Trigger;

public static class TriggerStartBuilder
{
    [ElementBuilder(ElementTypes = ["util-triggerstart"])]
    public static void Build(GameObject gameObject)
    {
        // TODO: Add onHideAndSeekStart & onClassicStart
        var trigger = gameObject.AddComponent<LITriggerSpawnable>();
        trigger.SetTrigger(gameObject, "onStart");
        gameObject.SetActive(true);
    }
}