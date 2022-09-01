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
     *      Adds downloaded maps to the lobby menu
     */
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.InitializeOptions))]
    public static class LobbyMenuInitPatch
    {
        public static string[] mapIDs = new string[0];
        public static int MAP_ID_OFFSET = 10;

        public static void Postfix(GameSettingMenu __instance)
        {
            mapIDs = MapLoader.GetMapIDs();
            for (int i = 0; i < __instance.AllItems.Length; i++)
            {
                Transform t = __instance.AllItems[i];
                if (!t.name.Equals("MapName", StringComparison.OrdinalIgnoreCase))
                    continue;

                KeyValueOption keyComp = t.GetComponent<KeyValueOption>();

                int mapIndex = MAP_ID_OFFSET;
                for (int o = 0; o < mapIDs.Length; o++)
                {
                    LIMetadata metadata = MapLoader.GetMetadata(mapIDs[o]);
                    if (string.IsNullOrEmpty(metadata.authorID))
                        continue;
                    var pair = new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>();
                    pair.key = metadata.name;
                    pair.value = mapIndex++;
                    keyComp.Values.Add(pair);
                }
                return;
            }
        }
    }

    /*
     *      Adjust for custom map values
     */
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.ValueChanged))]
    public static class LobbyMenuChangePatch
    {
        public static bool Prefix([HarmonyArgument(0)] OptionBehaviour option, GameOptionsMenu __instance)
        {
            if (option.Title != StringNames.GameMapName)
                return true;

            int mapID = option.GetInt();
            GameOptionsData gameOptions = PlayerControl.GameOptions;
            if (mapID < LobbyMenuInitPatch.MAP_ID_OFFSET)
            {
                gameOptions.MapId = (byte)mapID;
            }
            else
            {
                gameOptions.MapId = 2;
                MapLoader.LoadMap(LobbyMenuInitPatch.mapIDs[mapID - LobbyMenuInitPatch.MAP_ID_OFFSET]);
            }

            if (PlayerControl.LocalPlayer)
            {
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
            }

            return false;
        }
    }
}
