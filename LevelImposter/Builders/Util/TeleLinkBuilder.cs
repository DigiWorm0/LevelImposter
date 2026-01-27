using System;
using System.Collections.Generic;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class TeleLinkBuilder : IElemBuilder
{
    public int Priority => IElemBuilder.LOW_PRIORITY; // <-- Run AFTER teleporters are built

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-tele")
            return;

        // Get Target Teleporter
        var targetID = elem.properties.teleporter;
        if (targetID == null)
            return;
        
        var targetTeleporterGameObject = LIBaseShip.Instance?.MapObjectDB.GetObject((Guid)targetID);
        var targetTeleporter = targetTeleporterGameObject?.GetComponent<LITeleporter>();

        // Get Teleporter
        var teleporter = obj.GetComponent<LITeleporter>();
        if (teleporter == null || targetTeleporter == null)
            return;

        // Set Target Teleporter
        teleporter.SetTargetTeleporter(targetTeleporter);
    }
}