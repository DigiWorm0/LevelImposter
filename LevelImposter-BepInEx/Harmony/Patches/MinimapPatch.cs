using HarmonyLib;
using LevelImposter.MinimapGen;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake))]
    public static class MinimapPatch
    {
        private static MinimapGenerator mapApplicator = new MinimapGenerator();

        public static void Prefix(MapBehaviour __instance)
        {
            mapApplicator.PreGen(__instance);
        }

        public static void Postfix(MapBehaviour __instance)
        {
            mapApplicator.Finish();
        }
    }
}
