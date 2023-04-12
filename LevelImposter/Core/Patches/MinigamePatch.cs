using HarmonyLib;
using System.Text;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Loads custom minigame sprites
     */
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    public static class MinigamePatch
    {
        private static Console? _lastConsole = null;

        public static void Postfix(Minigame __instance)
        {
            var currentConsole = __instance.Console ?? _lastConsole;
            var minigameSprites = currentConsole?.GetComponent<MinigameSprites>();
            minigameSprites?.LoadMinigame(__instance);

            _lastConsole = currentConsole;
        }
    }
    [HarmonyPatch(typeof(MultistageMinigame), nameof(MultistageMinigame.Begin))]
    public static class MultistageMinigamePatch
    {
        public static void Postfix(MultistageMinigame __instance)
        {
            MinigamePatch.Postfix(__instance);
        }
    }
}
