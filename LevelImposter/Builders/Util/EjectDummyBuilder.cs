using System;
using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders;

public class EjectDummyBuilder : IElemBuilder
{
    public EjectDummyBuilder()
    {
        PoolablePlayers.Clear();
    }

    public static List<PoolablePlayer> PoolablePlayers { get; } = new();

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-ejectdummy")
            return;

        // Get Eject Controller Prefab
        var skeldPrefab = AssetDB.GetObject("ss-skeld");
        var skeldShipStatus = skeldPrefab?.GetComponent<ShipStatus>();
        var skeldEjectController = skeldShipStatus?.ExileCutscenePrefab;
        if (!skeldEjectController)
            throw new Exception("Failed to get Eject Controller from Skeld's ShipStatus");

        // Get Player Prefab
        var playerPrefab = skeldEjectController?.Player;
        if (!playerPrefab)
            throw new Exception("Failed to get Player Prefab from Skeld's Eject Controller");

        // Clone Prefab to Object
        var player = Object.Instantiate(playerPrefab, obj.transform);
        player.transform.localPosition = Vector3.zero;
        player.transform.localScale = Vector3.one;
        player.transform.localRotation = Quaternion.identity;

        // Set Layer
        player.gameObject.SetLayerOfChildren((int)Layer.Ship);

        // Update Cosmetics on Sprite Load
        if (SpriteLoader.Instance == null)
        {
            LILogger.Warn("Spite Loader is not instantiated");
            return;
        }

        // Add to PoolablePlayers
        PoolablePlayers.Add(player);

        // Update Cosmetics on Sprite Load
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != elem.id || player == null)
                return;

            player.cosmetics.skin.layer.sprite = spriteRenderer.sprite;
        };
    }
}