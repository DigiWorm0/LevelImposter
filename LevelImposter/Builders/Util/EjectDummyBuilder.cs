using System;
using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Builders.Generic;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders.Util;

internal static class EjectDummyBuilder
{
    public enum PlayerDummyType
    {
        Floating,
        Standing
    }

    public static List<PlayerDummy> PlayerDummies { get; } = [];

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        PlayerDummies.Clear();
    }

    [ElementBuilder(ElementTypes = ["util-ejectdummy", "util-ejectdummy2"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Get Type
        var type = element.type switch
        {
            "util-ejectdummy" => PlayerDummyType.Floating,
            "util-ejectdummy2" => PlayerDummyType.Standing,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Get Eject Controller Prefab
        var shipPrefab = PrefabDB.GetObject(type == PlayerDummyType.Floating ? "ss-skeld" : "ss-fungle");
        var shipStatusPrefab = shipPrefab?.GetComponent<ShipStatus>();
        var ejectControllerPrefab = shipStatusPrefab?.ExileCutscenePrefab;
        if (!ejectControllerPrefab)
            throw new Exception("Failed to get Eject Controller from Skeld's ShipStatus");

        // Get Player Prefab
        var playerPrefab = ejectControllerPrefab?.Player;
        if (!playerPrefab)
            throw new Exception("Failed to get Player Prefab from Skeld's Eject Controller");

        // Clone Prefab to Object
        var player = Object.Instantiate(playerPrefab, gameObject.transform);
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
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        SpriteBuilder.OnSpriteLoad += (loadedElem, _) =>
        {
            if (loadedElem.id != element.id || player == null)
                return;

            player.cosmetics.skin.layer.sprite = spriteRenderer.sprite;
        };

        // Hide Object By Default
        player.gameObject.SetActive(false);
    }

    public readonly struct PlayerDummy(PoolablePlayer poolablePlayer, PlayerDummyType type)
    {
        public PoolablePlayer PoolablePlayer => poolablePlayer;
        public PlayerDummyType Type => type;
    }
}