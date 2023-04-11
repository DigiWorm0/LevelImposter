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
    [HarmonyPatch(typeof(MultistageMinigame), nameof(MultistageMinigame.Begin))]
    public static class MinigamePatch
    {
        public static void Postfix(Minigame __instance)
        {
            var minigameSprites = __instance.Console?.GetComponent<MinigameSprites>();
            minigameSprites?.LoadMinigame(__instance);
        }
    }
}
