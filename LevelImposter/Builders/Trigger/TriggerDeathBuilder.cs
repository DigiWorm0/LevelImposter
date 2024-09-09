using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class TriggerDeathBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-triggerdeath")
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Trigger Area
        var deathArea = obj.AddComponent<LIDeathArea>();
        deathArea.SetCreateDeadBody(elem.properties.createDeadBody ?? true);
    }
}