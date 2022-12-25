using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    /*
     *      Unlike SurveillanceMinigame (Skeld's camera panel),
     *      PlanetSurveillanceMinigame (Polus's camera panel)
     *      ignores SurvCamera.CamSize. This fixes that.
     */
    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.NextCamera))]
    public static class PolusCamPatch
    {
        public static void Postfix(PlanetSurveillanceMinigame __instance)
        {
            SurvCamera survCamera = __instance.survCameras[__instance.currentCamera];

            // Size
            __instance.Camera.orthographicSize = survCamera.CamSize;

            // Clear Screen
            __instance.Camera.clearFlags = CameraClearFlags.SolidColor;
            __instance.Camera.backgroundColor = Camera.main.backgroundColor;

            // Z Index
            Vector3 pos = __instance.Camera.transform.position;
            __instance.Camera.transform.position = new Vector3(
                pos.x,
                pos.y,
                -0.1f
            );
        }
    }
}
