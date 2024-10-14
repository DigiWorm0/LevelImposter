using System;
using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders;

public class EjectDummyBuilder : IElemBuilder
{
    public enum PlayerDummyType
    {
        Floating,
        Standing
    }

    public EjectDummyBuilder()
    {
        PlayerDummies.Clear();
    }

    public static List<PlayerDummy> PlayerDummies { get; } = new();

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("util-ejectdummy"))
            return;

        // Get Type
        var type = elem.type switch
        {
            "util-ejectdummy" => PlayerDummyType.Floating,
            "util-ejectdummy2" => PlayerDummyType.Standing,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Get Eject Controller Prefab
        var skeldPrefab = AssetDB.GetObject(type == PlayerDummyType.Floating ? "ss-skeld" : "ss-fungle");
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
        if (!player)
            throw new Exception("Failed to clone Player Prefab");

        // Reset Transform
        player!.transform.localPosition = Vector3.zero;
        player.transform.localScale = Vector3.one;
        player.transform.localRotation = Quaternion.identity;

        // Set Layer
        player.gameObject.SetLayerOfChildren((int)Layer.Ship);

        // Add to PoolablePlayers
        PlayerDummies.Add(new PlayerDummy(player, type));

        // Update Cosmetics on Sprite Load
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        SpriteBuilder.OnSpriteLoad += (loadedElem, _) =>
        {
            if (loadedElem.id != elem.id || player == null)
                return;

            player.cosmetics.skin.layer.sprite = spriteRenderer.sprite;
        };

        // Hide Object By Default
        player.gameObject.SetActive(false);
    }

    public readonly struct PlayerDummy(PoolablePlayer _poolablePlayer, PlayerDummyType _type)
    {
        public PoolablePlayer PoolablePlayer => _poolablePlayer;
        public PlayerDummyType Type => _type;
    }
}