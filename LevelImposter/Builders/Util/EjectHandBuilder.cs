using System;
using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

public class EjectHandBuilder : IElemBuilder
{
    public static List<SpriteRenderer> AllHands { get; } = [];
    
    public void OnPreBuild()
    {
        AllHands.Clear();
    }
    
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-ejecthand" &&
            elem.type != "util-ejectthumb")
            return;

        // Get Eject Controller Prefab
        var polusPrefab = AssetDB.GetObject("ss-polus");
        var polusShipStatus = polusPrefab?.GetComponent<ShipStatus>();
        var polusEjectController = polusShipStatus?.ExileCutscenePrefab?.TryCast<PbExileController>();
        if (!polusEjectController)
            throw new Exception("Failed to get Eject Controller from Polus's ShipStatus");

        // Get Hand Prefab
        var handPrefab = polusEjectController?.HandSlot;
        if (!handPrefab)
            throw new Exception("Failed to get Player Prefab from Skeld's Eject Controller");

        // Clone Sprite to Object
        var hand = MapUtils.CloneSprite(obj, handPrefab?.gameObject);

        // Update Sprite (Thumb or Hand)
        var isThumb = elem.type == "util-ejectthumb";
        hand.sprite = isThumb ? polusEjectController?.GoodHand : polusEjectController?.BadHand;

        // Add to Hands
        AllHands.Add(hand);

        // Hide Object By Default
        obj.SetActive(false);
    }
}