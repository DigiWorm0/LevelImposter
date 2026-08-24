using System;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.DB;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders.Util;

internal static class EjectBuilder
{
    public static LIExileController? EjectController { get; private set; }

    [MapBuilder(Priority = Priority.FIRST, Target = MapTarget.Game)]
    public static void Reset()
    {
        EjectController = null;
    }

    [ElementBuilder(ElementTypes = ["util-eject"])]
    public static void Build(LIBaseShip baseShip, GameObject gameObject)
    {
        // Move to prefab container
        gameObject.transform.SetParent(baseShip.Prefabs.Container);

        // Get Eject Controller Prefab
        var skeldPrefab = PrefabDB.GetObject("ss-skeld");
        var skeldShipStatus = skeldPrefab?.GetComponent<ShipStatus>();
        var skeldEjectController = skeldShipStatus?.ExileCutscenePrefab;
        if (!skeldEjectController)
            throw new Exception("Failed to get Eject Controller from Skeld's ShipStatus");

        // Copy Components from Skeld's Prefab
        var impostorText = Object.Instantiate(skeldEjectController?.ImpostorText, gameObject.transform);
        var text = Object.Instantiate(skeldEjectController?.Text, gameObject.transform);
        var judgeText = Object.Instantiate(skeldEjectController?.judgeText, gameObject.transform);
        var player = Object.Instantiate(skeldEjectController?.Player, gameObject.transform);

        // TODO: Hide Player

        // Create Eject Controller
        EjectController = gameObject.AddComponent<LIExileController>();
        EjectController.ImpostorText = impostorText;
        EjectController.Text = text;
        EjectController.judgeText = judgeText;
        EjectController.Player = player;
        EjectController.TextSound = skeldEjectController?.TextSound;

        // Add to ShipStatus
        var shipStatus = LIShipStatus.GetShip();
        shipStatus.ExileCutscenePrefab = EjectController;
    }
}