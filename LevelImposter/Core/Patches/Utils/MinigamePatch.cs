using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Loads all minigame sprites when a minigame is started.
    /// </summary>
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    public static class MinigamePatch
    {
        // Set by LevelImposter.ConsolePatch
        public static GameObject? LastConsole = null;

        public static void Postfix(Minigame __instance)
        {
            if (LIShipStatus.Instance == null)
                return;

            // Get Component
            var currentConsole = __instance.Console?.gameObject ?? LastConsole;
            var minigameSprites = currentConsole?.GetComponent<MinigameSprites>();

            // Handle Doors
            if (minigameSprites == null)
            {
                var doorConsole = currentConsole?.GetComponent<DoorConsole>();
                minigameSprites = doorConsole?.MyDoor?.GetComponent<MinigameSprites>();
            }

            // Load Minigame
            minigameSprites?.LoadMinigame(__instance);
        }
    }

    /// <summary>
    /// When a minigame is closed, clear the last console.
    /// </summary>
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new System.Type[0])]
    public static class MinigameClosePatch
    {
        public static void Postfix()
        {
            MinigamePatch.LastConsole = null;
        }
    }
}
