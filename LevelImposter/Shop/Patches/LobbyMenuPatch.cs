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
        public static bool UpdateFix(KeyValueOption __instance)
        {
            if (__instance.Title == StringNames.GameMapName && MapLoader.CurrentMap != null && __instance.oldValue != __instance.Selected)
            {
                for (int i = __instance.Values.Count - 1; i >= 0; i--)
                    if (__instance.Values[i].Key == "LevelImposter")
                        __instance.Values.RemoveAt(i);

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

    /*
     *      Initializes a new Map Console in the Lobby
     */
    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Refresh))]
    public static class LobbyOptionsRefreshPatch
    {
        public static void Prefix(CreateOptionsPicker __instance)
        {
            var options = __instance.GetTargetOptions();
            if (Constants.MapNames[options.MapId] == "LevelImposter")
            {
                options.SetByte(AmongUs.GameOptions.ByteOptionNames.MapId, (byte)MapType.Skeld);
                __instance.SetTargetOptions(options);
            }
        }
    }
}
