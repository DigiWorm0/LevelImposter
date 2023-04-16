using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;
using InnerNet;

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
            if (MapUtils.GetCurrentMapType() == MapType.LevelImposter)
                __instance.gameObject.AddComponent<LIShipStatus>();
        }
    }

    /*
     *      Syncs all players' games
     */
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendClientReady))]
    public static class ReadyPatch
    {
        public static bool Prefix(InnerNetClient __instance)
        {
            if (LIShipStatus.Instance == null || LIShipStatus.Instance.IsReady)
                return true;

            MapUtils.WaitForShip(() =>
            {
                __instance.SendClientReady();
            });
            return false;
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
