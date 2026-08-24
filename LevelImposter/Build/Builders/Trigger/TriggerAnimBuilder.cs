using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using UnityEngine;

namespace LevelImposter.Build.Builders.Trigger;

public static class TriggerAnimBuilder
{
    [ElementBuilder(ElementTypes = ["util-triggeranim"])]
    public static void Build(GameObject gameObject)
    {
        gameObject.AddComponent<TriggerAnim>();
    }
}