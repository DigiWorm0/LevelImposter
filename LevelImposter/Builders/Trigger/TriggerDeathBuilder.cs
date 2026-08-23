using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Trigger;

public static class TriggerDeathBuilder
{
    [ElementBuilder(ElementTypes = ["util-triggerdeath"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Trigger Area
        var deathArea = gameObject.AddComponent<LIDeathArea>();
        deathArea.SetCreateDeadBody(element.properties.createDeadBody ?? true);
    }
}