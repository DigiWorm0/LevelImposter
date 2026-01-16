using AmongUs.GameOptions;
using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop;

/*
 *      Randomized the map when using a vanilla map selection
 */
[HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SelectMap), typeof(int))]
public static class RandomizeMapPatch
{
    public static void Postfix()
    {
        // If a vanilla map is selected, ensure the map is randomized
        if (GameConfiguration.CurrentMapType != MapType.LevelImposter && 
            !GameConfiguration.HideMapName)
            MapRandomizer.RandomizeMap(false);
    }
}