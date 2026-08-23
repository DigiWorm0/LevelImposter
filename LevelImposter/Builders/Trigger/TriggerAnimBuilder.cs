using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Trigger;

public static class TriggerAnimBuilder
{
    [ElementBuilder(ElementTypes = ["util-triggeranim"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        gameObject.AddComponent<TriggerAnim>();
    }
}