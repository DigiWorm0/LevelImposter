using HarmonyLib;
using LevelImposter.Shop;
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
        if(MapLoader.CurrentMap == null)
            return;

        // Since the game is in freeplay, removing the local player just leaves all the dummy PlayerControls
        var dummyPlayers = PlayerControl.AllPlayerControls.ToArray().Where(p => p != PlayerControl.LocalPlayer).ToArray();

        // Finds a dummy element that has a corresponding location index (see DummyBuilder)
        // which matches this DummyBehaviour's PlayerControl
        foreach (var elem in MapLoader.CurrentMap.elements)
        {
            if(elem.type != "util-dummy")
                continue;
            if (!LIShipStatus.GetInstance().DummyIndex.TryGetValue(elem.id, out int index))
                continue;
            if (__instance.myPlayer != dummyPlayers[index])
                continue;

            CustomizeDummy(__instance, elem);
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