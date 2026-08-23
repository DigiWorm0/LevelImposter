using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Trigger;

public static class TriggerAreaBuilder
{
    [ElementBuilder(ElementTypes = ["util-triggerarea"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Ghost
        if (element.properties.isGhostEnabled ?? false)
            gameObject.layer = (int)Layer.Default;

        // Trigger Area
        var triggerArea = gameObject.AddComponent<LITriggerArea>();
        triggerArea.SetClientSide(element.properties.triggerClientSide != false);
    }
}