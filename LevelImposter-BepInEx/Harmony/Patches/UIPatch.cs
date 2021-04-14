using HarmonyLib;
using LevelImposter.Map;
using LevelImposter.MinimapGen;
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

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
    public static class AdminTablePatch
    {
        public static void Prefix(HudManager __instance)
        {
            BGGenerator.SetColor(new Color(0, 1.0f, 0, 0.6f));
        }
    }

    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.OnEnable))]
    public static class SabotagePatch
    {
        public static void Prefix(HudManager __instance)
        {
            BGGenerator.SetColor(new Color(1.0f, 0, 0, 0.6f));
        }
    }

    [HarmonyPatch(typeof(MapTaskOverlay), nameof(MapTaskOverlay.Show))]
    public static class MiniMapPatch
    {
        public static void Prefix(HudManager __instance)
        {
            BGGenerator.SetColor(new Color(0, 0, 1.0f, 0.6f));
        }
    }
}
