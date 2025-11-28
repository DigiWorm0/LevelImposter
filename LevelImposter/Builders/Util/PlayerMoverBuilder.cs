using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class PlayerMoverBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-playermover")
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Add Component
        obj.AddComponent<LIPlayerMover>();
    }
}