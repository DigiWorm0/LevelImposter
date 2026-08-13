using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Core.Patches.Fixes;

/// <summary>
///     Increases the transition fade size to work with larger maps
/// </summary>
[HarmonyPatch(typeof(TransitionFade), nameof(TransitionFade.DoTransitionFade))]
public static class TransitionSizePatch
{
    public static void Postfix(TransitionFade __instance)
    {
        __instance.overlay.transform.localScale = new Vector3(
            1000.0f,
            1000.0f,
            4.0f
        );
    }
}