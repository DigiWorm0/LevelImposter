using System;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class TeleBuilder
{
    [ElementBuilder(ElementTypes = ["util-tele"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;
        if (element.properties.isGhostEnabled ?? true)
            gameObject.layer = (int)Layer.Default;

        // Teleporter
        gameObject.AddComponent<LITeleporter>();
    }

    [ElementBuilder(
        Priority = Priority.LAST,
        ElementTypes = ["util-tele"]
    )]
    public static void LinkTeleporters(LIElement element, GameObject gameObject)
    {
        // Get Target Teleporter
        var targetID = element.properties.teleporter;
        if (targetID == null)
            return;

        var targetTeleporterGameObject = LIBaseShip.Instance?.MapObjectDB.GetObject((Guid)targetID);
        var targetTeleporter = targetTeleporterGameObject?.GetComponent<LITeleporter>();

        // Get Teleporter
        var teleporter = gameObject.GetComponent<LITeleporter>();
        if (teleporter == null || targetTeleporter == null)
            return;

        // Set Target Teleporter
        teleporter.SetTargetTeleporter(targetTeleporter);
    }
}