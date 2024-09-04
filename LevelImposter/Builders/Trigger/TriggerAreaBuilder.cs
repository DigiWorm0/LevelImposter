using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class TriggerAreaBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-triggerarea")
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Ghost
        if (elem.properties.isGhostEnabled ?? false)
            obj.layer = (int)Layer.Default;

        // Trigger Area
        var triggerArea = obj.AddComponent<LITriggerArea>();
        triggerArea.SetClientSide(elem.properties.triggerClientSide != false);
    }

    public void PostBuild()
    {
    }
}