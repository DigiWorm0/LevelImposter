using System;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders;

public class EjectBuilder : IElemBuilder
{
    public static LIExileController? EjectController { get; private set; }

    public void OnPreBuild()
    {
        EjectController = null;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-eject")
            return;

        // Create container object (This will be a prefab)
        var container = new GameObject("EjectContainer");
        container.transform.SetParent(obj.transform.parent);
        container.SetActive(false);
        obj.transform.SetParent(container.transform);

        // Get Eject Controller Prefab
        var skeldPrefab = AssetDB.GetObject("ss-skeld");
        var skeldShipStatus = skeldPrefab?.GetComponent<ShipStatus>();
        var skeldEjectController = skeldShipStatus?.ExileCutscenePrefab;
        if (!skeldEjectController)
            throw new Exception("Failed to get Eject Controller from Skeld's ShipStatus");

        // Copy Components from Skeld's Prefab
        var impostorText = Object.Instantiate(skeldEjectController?.ImpostorText, obj.transform);
        var text = Object.Instantiate(skeldEjectController?.Text, obj.transform);
        var player = Object.Instantiate(skeldEjectController?.Player, obj.transform);

        // TODO: Hide Player

        // Create Eject Controller
        EjectController = obj.AddComponent<LIExileController>();
        EjectController.ImpostorText = impostorText;
        EjectController.Text = text;
        EjectController.Player = player;
        EjectController.TextSound = skeldEjectController?.TextSound;

        // Add to ShipStatus
        var shipStatus = LIShipStatus.GetShip();
        shipStatus.ExileCutscenePrefab = EjectController;
    }
}