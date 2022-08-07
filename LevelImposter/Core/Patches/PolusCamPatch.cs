using HarmonyLib;
using System;
using System.Collections.Generic;
using UnhollowerBaseLib.Attributes;
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
            __instance.Camera.orthographicSize = survCamera.CamSize;
        }
    }
}
