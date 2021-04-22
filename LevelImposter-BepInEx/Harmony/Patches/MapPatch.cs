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

        public static bool Prefix(PolusShipStatus __instance)
        {
            ShipStatus.Instance = __instance;
            mapApplicator.PreBuild(__instance);
            __instance.AssignTaskIndexes();
            return false;
        }

        public static void Postfix(PolusShipStatus __instance)
        {
            mapApplicator.PostBuild(__instance);
        }
    }

    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    public static class ShipEnableFix
    {
        public static bool Prefix(PolusShipStatus __instance)
        {
            Camera main = Camera.main;
            main.backgroundColor = __instance.CameraColor;
            FollowerCamera component = main.GetComponent<FollowerCamera>();
            DestroyableSingleton<HudManager>.Instance.ShadowQuad.material.SetInt("_Mask", 7);
            if (component)
            {
                component.shakeAmount = 0f;
                component.shakePeriod = 0f;
            }

            return false;
        }
    }
}
