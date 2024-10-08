using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop;

/*
 *      Switches custom map to fallback
 *      if a vanilla map is selected
 */
[HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SelectMap), typeof(int))]
public static class MapChangePatch
{
    public static void Postfix()
    {
        if (GameState.IsCustomMapSelected)
            return;

        MapLoader.SetFallback(true);
        MapSync.SyncMapID();
    }
}