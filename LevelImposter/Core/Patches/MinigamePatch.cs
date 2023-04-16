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
        public static Console? LastConsole = null;

        public static void Postfix(Minigame __instance)
        {
            var currentConsole = __instance.Console ?? LastConsole;
            var minigameSprites = currentConsole?.GetComponent<MinigameSprites>();
            minigameSprites?.LoadMinigame(__instance);

            LastConsole = currentConsole;
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
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new System.Type[0])]
    public static class MinigameClosePatch
    {
        public static void Postfix(MultistageMinigame __instance)
        {
            MinigamePatch.LastConsole = null;
        }
    }
}
