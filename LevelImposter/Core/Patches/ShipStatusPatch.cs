using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Appends a new LIShipStatus
     *      to all ShipStatus-es
     */
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusPatch
    {
        public static void Prefix(ShipStatus __instance)
        {
            __instance.gameObject.AddComponent<LIShipStatus>();
        }
    }

    /*
     *      Unloads maps after disconnect
     */
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    public static class ExitGamePatch
    {
        public static void Postfix()
        {
            MapLoader.UnloadMap();
        }
    }
}
