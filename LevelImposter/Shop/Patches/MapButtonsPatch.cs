using AmongUs.GameOptions;
using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop;

/*
 *      Prevents NullReferenceException when
 *      creating a new lobby
 */
[HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SelectMap), typeof(int))]
public static class MapButtonsPatch
{
    public static bool Prefix(GameOptionsMapPicker __instance)
    {
        // Check if the map is custom
        if (!GameState.IsCustomMapSelected)
            return true;

        // Switches the map to Skeld
        GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, (byte)MapType.Skeld);

        // Re-runs the method
        __instance.SelectMap((byte)MapType.Skeld);

        // Aborts the method
        return false;
    }
}