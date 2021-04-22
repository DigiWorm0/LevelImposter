using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LevelImposter.Harmony.Patches
{
    
    [HarmonyPatch(typeof(AspectSize), nameof(AspectSize.OnEnable))]
    public static class AspectPatch
    {
        public static bool Prefix()
        {
            return ShipStatus.Instance.name != "PolusShip(Clone)";
        }
    }
    
    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    public static class MapPatch
    {
        private static MapApplicator mapApplicator = new MapApplicator();

        public static void Prefix(PolusShipStatus __instance)
        {
            mapApplicator.PreBuild(__instance);
        }

        public static void Postfix(PolusShipStatus __instance)
        {
            mapApplicator.PostBuild(__instance);
        }

    }
}
