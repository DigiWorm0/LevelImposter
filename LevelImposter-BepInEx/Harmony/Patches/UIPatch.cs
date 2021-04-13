using HarmonyLib;
using LevelImposter.Map;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(AspectSize), nameof(AspectSize.OnEnable))]
    public static class AspectPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.OnEnable))]
    public static class SabPatch1
    {
        public static bool Prefix()
        {
            return false; // TODO
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMap))]
    public static class HudPatch
    {
        public static void Prefix(HudManager __instance)
        {
            MapGenerator.SetColor(new Color(0, 0, 1.0f, 0.6f));
        }
    }
}
