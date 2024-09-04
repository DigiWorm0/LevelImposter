using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Applies <c>SurvCamera.CamSize</c> to <c>PlanetSurveillanceMinigame.Camera</c>.
/// </summary>
[HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.NextCamera))]
public static class PolusCamPatch
{
    public static void Postfix(PlanetSurveillanceMinigame __instance)
    {
        if (LIShipStatus.IsInstance())
            return;

        // Get the Camera
        var survCamera = __instance.survCameras[__instance.currentCamera];

        // Set Size
        __instance.Camera.orthographicSize = survCamera.CamSize;

        // Set Screen Clear
        __instance.Camera.clearFlags = CameraClearFlags.SolidColor;
        __instance.Camera.backgroundColor = Camera.main.backgroundColor;

        // Set Z Index
        var pos = __instance.Camera.transform.position;
        __instance.Camera.transform.position = new Vector3(
            pos.x,
            pos.y,
            -0.1f
        );
    }
}