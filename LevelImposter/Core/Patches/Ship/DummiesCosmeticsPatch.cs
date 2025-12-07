using HarmonyLib;
using LevelImposter.Shop;
using System.Linq;

namespace LevelImposter.Core;

/// <summary>
///     Renames all dummies to the names of
///     their corresponding objects in the editor
/// </summary>
[HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Start))]
public static class DummiesCosmeticsPatch
{
    public static void Postfix(DummyBehaviour __instance)
    {
        // Only execute in custom freeplay maps
        if (!LIShipStatus.IsInstance())
            return;
        if (!GameState.IsInFreeplay)
            return;

        // Get all dummy elements on the map
        LIElement[] dummyElements = MapLoader.CurrentMap.elements.Where(element => element.type == "util-dummy").ToArray();
        // Determine which one this dummy belongs to
        // (subtract 1 to account for the player PlayerControl)
        LIElement thisElement = dummyElements[PlayerControl.AllPlayerControls.IndexOf(__instance.myPlayer) - 1];

        __instance.myPlayer.SetName(thisElement.name);
    }
}