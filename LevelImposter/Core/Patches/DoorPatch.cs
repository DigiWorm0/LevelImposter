using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;
using PowerTools;

namespace LevelImposter.Core
{
    /*
     *      This enables the use of
     *      LevelImposter.Core.GIFAnimator
     *      instead of PowerTools.SpriteAnim
     */
    [HarmonyPatch(typeof(PlainDoor), nameof(PlainDoor.SetDoorway))]
    public static class DoorPatch
    {
        public static void Postfix([HarmonyArgument(0)] bool open, PlainDoor __instance)
        {
            if (MapLoader.CurrentMap == null)
                return;

            Collider2D[] colliders = __instance.gameObject.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
                collider.enabled = !open;

            SpriteRenderer spriteRenderer = __instance.GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = !open;

            BoxCollider2D dummyCollider = __instance.GetComponent<BoxCollider2D>();
            dummyCollider.enabled = false;
        }
    }
}