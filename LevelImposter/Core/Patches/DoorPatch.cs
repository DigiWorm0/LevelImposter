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
        private static bool _hasStateChanged = false;

        public static void Prefix([HarmonyArgument(0)] bool open, PlainDoor __instance)
        {
            _hasStateChanged = open != __instance.Open;
        }

        public static void Postfix([HarmonyArgument(0)] bool open, PlainDoor __instance)
        {
            if (MapLoader.CurrentMap == null)
                return;

            // Colliders
            Collider2D[] colliders = __instance.gameObject.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
                collider.enabled = !open;

            // Dummy Collider
            BoxCollider2D dummyCollider = __instance.GetComponent<BoxCollider2D>();
            dummyCollider.enabled = false;

            // Sprite Renderer
            AnimationClip animClip = open ? __instance.OpenDoorAnim : __instance.CloseDoorAnim;
            SpriteAnim spriteAnim = __instance.GetComponent<SpriteAnim>();
            GIFAnimator gifAnim = __instance.GetComponent<GIFAnimator>();
            SpriteRenderer spriteRenderer = __instance.GetComponent<SpriteRenderer>();
            if (spriteAnim != null && animClip != null) 
            {
                // SpriteAnim
            }
            else if (gifAnim != null)
            {
                // GIFAnimator
                if (_hasStateChanged)
                    gifAnim.Play(false, open);
                spriteRenderer.enabled = true;
            }
            else
            {
                // SpriteRenderer
                spriteRenderer.enabled = !open;
            }

            // Triggers
            if (_hasStateChanged)
            {
                string triggerID = open ? "onOpen" : "onClose";
                LITriggerable.Trigger(__instance.gameObject, triggerID, PlayerControl.LocalPlayer);
            }

            return;
        }
    }
}