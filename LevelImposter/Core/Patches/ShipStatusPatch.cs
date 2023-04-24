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
            else if (!MapLoader.IsFallback)
                LILogger.Error("Another mod has changed the map.\nMake sure other map randomizers are disabled.");
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

            MapUtils.WaitForShip(LIConstants.MAX_LOAD_TIME, () =>
            {
                __instance.SendClientReady();
            });
            return false;
        }
    }

    /*
     *      Increase Max Wait Time
     */
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class WaitTimePatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            __instance.MAX_CLIENT_WAIT_TIME = LIConstants.CONNECTION_TIMEOUT;
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
