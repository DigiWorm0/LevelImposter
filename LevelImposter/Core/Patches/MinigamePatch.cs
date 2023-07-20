using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Core
{
    /*
     *      Loads custom minigame sprites
     */
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    public static class MinigamePatch
    {
        public static GameObject? LastConsole = null; // Set by ConsolePatch

        public static void Postfix(Minigame __instance)
        {
            var currentConsole = __instance.Console?.gameObject ?? LastConsole;
            var minigameSprites = currentConsole?.GetComponent<MinigameSprites>();
            minigameSprites?.LoadMinigame(__instance);
        }
    }
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new System.Type[0])]
    public static class MinigameClosePatch
    {
        public static void Postfix(MultistageMinigame __instance)
        {
            MinigamePatch.LastConsole = null;
        }
    }
}
