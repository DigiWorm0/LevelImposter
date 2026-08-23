using System;
using System.Collections.Generic;
using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class EjectHandBuilder
{
    private const string EJECT_HAND_TYPE = "util-ejecthand";
    private const string EJECT_THUMB_TYPE = "util-ejectthumb";
    public static List<SpriteRenderer> AllHands { get; } = [];

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        AllHands.Clear();
    }

    [ElementBuilder(ElementTypes = [EJECT_HAND_TYPE, EJECT_THUMB_TYPE])]
    public static void OnBuild(LIElement element, GameObject gameObject)
    {
        // Get Eject Controller Prefab
        var polusPrefab = PrefabDB.GetObject("ss-polus");
        var polusShipStatus = polusPrefab?.GetComponent<ShipStatus>();
        var polusEjectController = polusShipStatus?.ExileCutscenePrefab?.TryCast<PbExileController>();
        if (!polusEjectController)
            throw new Exception("Failed to get Eject Controller from Polus's ShipStatus");

        // Get Hand Prefab
        var handPrefab = polusEjectController?.HandSlot;
        if (!handPrefab)
            throw new Exception("Failed to get Player Prefab from Skeld's Eject Controller");

        // Clone Sprite to Object
        var hand = gameObject.CloneSprite(handPrefab?.gameObject);

        // Update Sprite (Thumb or Hand)
        var isThumb = element.type == EJECT_THUMB_TYPE;
        hand.sprite = isThumb ? polusEjectController?.GoodHand : polusEjectController?.BadHand;

        // Add to Hands
        AllHands.Add(hand);

        // Hide Object By Default
        gameObject.SetActive(false);
    }
}