using HarmonyLib;
using LevelImposter.Trigger;
using PowerTools;
using UnityEngine;

namespace LevelImposter.Core;

//// <summary>
/// Toggles a GIF animation alongside the
/// regular animation components on doors.
/// </summary>
[HarmonyPatch(typeof(PlainDoor), nameof(PlainDoor.SetDoorway))]
public static class DoorPatch
{
    private static bool _hasStateChanged;

    public static void Prefix([HarmonyArgument(0)] bool open, PlainDoor __instance)
    {
        _hasStateChanged = open != __instance.Open;
    }

    public static void Postfix([HarmonyArgument(0)] bool open, PlainDoor __instance)
    {
        if (!LIShipStatus.IsInstance())
            return;

        // Colliders
        Collider2D[] colliders = __instance.gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.enabled = !open;

        // Dummy Collider
        var dummyCollider = __instance.GetComponent<BoxCollider2D>();
        dummyCollider.enabled = false;

        // Sprite Renderer
        var animClip = open ? __instance.OpenDoorAnim : __instance.CloseDoorAnim;
        var spriteAnim = __instance.GetComponent<SpriteAnim>();
        var animator = __instance.GetComponent<LIAnimatorBase>();
        var spriteRenderer = __instance.GetComponent<SpriteRenderer>();
        if (spriteAnim != null && animClip != null)
        {
            // SpriteAnim
        }
        else if (animator != null)
        {
            // GIFAnimator
            if (_hasStateChanged)
                animator.PlayType(open ? "openDoor" : "closeDoor");
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
            var triggerID = open ? "onOpen" : "onClose";
            TriggerSignal signal = new(__instance.gameObject, triggerID, PlayerControl.LocalPlayer);
            TriggerSystem.GetInstance().FireTrigger(signal);
        }
    }
}