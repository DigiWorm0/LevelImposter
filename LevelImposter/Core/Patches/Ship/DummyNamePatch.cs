using HarmonyLib;
using LevelImposter.Shop;
using System.Linq;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Update the names of all dummy PlayerControls to match
///     the names of their corresponding elements in the editor.
/// </summary>
[HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Start))]
public static class DummyNamePatch
{
    public static void Postfix(DummyBehaviour __instance)
    {
        // Only applies to dummies on LI freeplay games
        if (!LIShipStatus.IsInstance())
            return;
        if (!GameState.IsInFreeplay)
            return;
        if(MapLoader.CurrentMap == null)
            return;

        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        if (shipStatus == null)
            return;

        var locations = shipStatus.DummyLocations;
        LIElement[] elements = MapLoader.CurrentMap.elements.Where(element => element.type == "util-dummy").ToArray();

        // Loop through all dummy locations added by DummyBuilder
        // and find which one corresponds to this dummy
        for (int i = 0; i < locations.Length; i++)
        {
            // Only check for matching X and Y positions
            if ((Vector2)__instance.transform.position == (Vector2)locations[i].position)
                CustomizeDummy(__instance, elements[i]);
        }
    }

    /// <summary>
    ///     Applies any customizations to the dummy game object using its editor element.
    /// </summary>
    private static void CustomizeDummy(DummyBehaviour dummy, LIElement element)
    {
        dummy.myPlayer.SetName(element.name);
    }
}