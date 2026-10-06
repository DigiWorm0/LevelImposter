using HarmonyLib;
using LevelImposter.Shop;
using LevelImposter.Core.Models;
using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;
using System.Linq;

namespace LevelImposter.Core;

/// <summary>
///     Update the names of all dummy PlayerControls to match
///     the names of their corresponding elements in the editor.
/// </summary>
[HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Start))]
public static class DummyPatch
{
    public static void Postfix(DummyBehaviour __instance)
    {
        // Only applies to dummies on LI freeplay games
        if (!LIShipStatus.IsInstance())
            return;
        if (!GameState.IsInFreeplay)
            return;
        if (GameConfiguration.CurrentMap == null)
            return;

        // Since the game is in freeplay, removing the local player just leaves all the dummy PlayerControls
        var dummyPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != PlayerControl.LocalPlayer)
            .ToArray();

        // Match custom dummy elements to freeplay dummy players by build/order index.
        var dummyElements = GameConfiguration.CurrentMap.elements
            .Where(e => e.type == "util-dummy")
            .ToArray();

        for (var index = 0; index < dummyElements.Length && index < dummyPlayers.Length; index++)
        {
            if (__instance.myPlayer != dummyPlayers[index])
                continue;

            CustomizeDummy(__instance, dummyElements[index]);
            break;
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
