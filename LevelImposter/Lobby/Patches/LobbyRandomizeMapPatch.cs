using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Lobby;

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