using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class TeleBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-tele")
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;
        if (elem.properties.isGhostEnabled ?? true)
            obj.layer = (int)Layer.Default;

        // Teleporter
        obj.AddComponent<LITeleporter>();
    }
}