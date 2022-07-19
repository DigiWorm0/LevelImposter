using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusPatch
    {
        public static void Prefix(ShipStatus __instance)
        {
            __instance.gameObject.AddComponent<LIShipStatus>();
        }
    }
}
