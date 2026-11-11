using HarmonyLib;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;

namespace LevelImposter.Core.Patches.Fixes;

/// <summary>
///     The culling mask for the binoculars camera excludes Physics Objects.
///     This patch adds the Physics layer to the binoculars camera culling mask.
/// </summary>
[HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
public class BinocularsCameraPatch
{
    public static void Postfix(FungleSurveillanceMinigame __instance)
    {
        if (!LIShipStatus.IsInstance())
            return;

        var camera = __instance.securityCamera.cam;
        camera.cullingMask |= 1 << (int)Layer.Physics;
    }
}