using System;
using System.Collections.Generic;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class TeleBuilder : IElemBuilder
{
    private readonly Dictionary<Guid, LITeleporter> _teleList = new();

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
        var teleporter = obj.AddComponent<LITeleporter>();
        _teleList[elem.id] = teleporter;
    }

    public void OnPostBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-tele")
            return;

        // Get Target Teleporter
        var targetID = elem.properties.teleporter;
        if (targetID == null)
            return;
        var targetTeleporter = _teleList.GetValueOrDefault((Guid)targetID);

        // Get Teleporter
        var teleporter = _teleList.GetValueOrDefault(elem.id);
        if (teleporter == null || targetTeleporter == null)
            return;

        // Set Target Teleporter
        teleporter.SetTargetTeleporter(targetTeleporter);
    }
}