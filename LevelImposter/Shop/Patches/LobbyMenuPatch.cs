using TMPro;
using HarmonyLib;
using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      Replace Map Name in Options Console
     */
    [HarmonyPatch(typeof(KeyValueOption))]
    public static class MapNameValuePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(KeyValueOption.FixedUpdate))]
        public static bool EnableFix(KeyValueOption __instance)
        {
            if (MapLoader.CurrentMap != null && __instance.Title == StringNames.GameMapName && __instance.oldValue != __instance.Selected)
            {
                __instance.oldValue = __instance.Selected;
                __instance.ValueText.text = MapLoader.CurrentMap.name;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(KeyValueOption.Increase))]
        [HarmonyPatch(nameof(KeyValueOption.Decrease))]
        public static void IncrementFix(KeyValueOption __instance)
        {
            if (__instance.Title == StringNames.GameMapName)
            {
                MapLoader.UnloadMap();
                MapUtils.SyncMapID();
            }
        }
    }

    /*
     *      Initializes a new Map Console in the Lobby
     */
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static class LobbyMenuInitPatch
    {
        public static void Postfix()
        {
            LobbyConsoleBuilder.Build();
        }
    }
}
