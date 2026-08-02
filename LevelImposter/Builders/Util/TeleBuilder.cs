using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Util;

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