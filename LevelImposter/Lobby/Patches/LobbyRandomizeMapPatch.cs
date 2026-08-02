using HarmonyLib;
using LevelImposter.Core;
using LevelImposter.Core.Models;
using LevelImposter.Lobby.Sync;

namespace LevelImposter.Lobby.Patches;

/*
 *      Randomizes the map when using a vanilla map selection screen in the lobby
 */
[HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SelectMap), typeof(int))]
public static class LobbyRandomizeMapPatch
{
    public static void Postfix()
    {
        // If a vanilla map is selected, ensure the map is randomized
        if (GameConfiguration.CurrentMapType != MapType.LevelImposter &&
            !GameConfiguration.HideMapName)
            MapRandomizer.RandomizeMap(false);
    }
}