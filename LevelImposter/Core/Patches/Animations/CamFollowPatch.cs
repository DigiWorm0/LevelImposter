using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Modifies the surveillance camera panel (util-cams*)
///     to follow a moving camera (util-cam)
/// </summary>
[HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
public static class CamFollowPatch
{
    public static void Postfix(PlanetSurveillanceMinigame __instance)
    {
        if (!LIShipStatus.IsInstance())
            return;

        var survCamera = __instance.survCameras[__instance.currentCamera];
        __instance.Camera.transform.position = survCamera.transform.position + survCamera.Offset;
    }
}