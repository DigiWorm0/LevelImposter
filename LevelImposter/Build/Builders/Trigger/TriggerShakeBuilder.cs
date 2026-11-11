using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Build.Builders.Trigger;

public static class TriggerShakeBuilder
{
    [ElementBuilder(ElementTypes = ["util-triggershake"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Trigger Area
        var shakeArea = gameObject.AddComponent<LIShakeArea>();
        shakeArea.SetParameters(
            element.properties.shakeAmount ?? 0.03f,
            element.properties.shakePeriod ?? 400.0f
        );
    }
}