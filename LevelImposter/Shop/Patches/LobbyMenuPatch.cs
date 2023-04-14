using TMPro;
using HarmonyLib;
using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

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
            bool isMapTitle = __instance.Title == StringNames.GameMapName;
            bool hasChanged = __instance.oldValue != __instance.Selected;
            bool shouldShowName = __instance.Selected == (int)MapType.LevelImposter || !MapLoader.IsFallback;
            bool isMapLoaded = MapLoader.CurrentMap != null;

            if (isMapTitle && hasChanged && shouldShowName && isMapLoaded)
            {
                __instance.oldValue = __instance.Selected;
                __instance.ValueText.text = MapLoader.IsFallback ? LIConstants.MAP_NAME : MapLoader.CurrentMap?.name;
                return false;
            }
            else if (isMapTitle && hasChanged && !isMapLoaded)
            {
                for (int i = __instance.Values.Count - 1; i >= 0; i--)
                    if (__instance.Values[i].Key == LIConstants.MAP_NAME)
                        __instance.Values.RemoveAt(i);
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
                MapSync.SyncMapID(!MapLoader.IsFallback);
                ConfigAPI.Instance?.SetLastMapID(null);
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

            // Load Last Map
            /*
            var lastMapID = ConfigAPI.Instance?.GetLastMapID();
            if (lastMapID != null && MapFileAPI.Instance?.Exists(lastMapID) == true)
                MapLoader.LoadMap(lastMapID, false, () => MapSync.SyncMapID(true));
            */
        }
    }

    /*
     *      Replaces the LI map name with the actual map name
     */
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    public static class StringRenamePatch
    {
        public static void Postfix(ref string __result)
        {
            if (MapLoader.CurrentMap == null || MapLoader.IsFallback) 
                return;

            __result = __result.Replace(LIConstants.MAP_NAME, MapLoader.CurrentMap.name);
        }
    }
    /*
     *      ???
     */
    /*
    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Refresh))]
    public static class LobbyOptionsRefreshPatch
    {
        public static void Prefix(CreateOptionsPicker __instance)
        {
            var options = __instance.GetTargetOptions();
            if (Constants.MapNames[options.MapId] == LIConstants.MAP_NAME)
            {
                options.SetByte(AmongUs.GameOptions.ByteOptionNames.MapId, (byte)MapType.Skeld);
                __instance.SetTargetOptions(options);
            }
        }
    }
    */
}
