using HarmonyLib;

namespace LevelImposter.Core
{
    /// <summary>
    /// Fixes a bug where WeatherSwitchGame renames
    /// all of the node minigame objects to their translated names.
    /// </summary>
    [HarmonyPatch(typeof(WeatherSwitchGame), nameof(WeatherSwitchGame.Start))]
    public class NodeMinigamePatch
    {
        public static void Postfix(WeatherSwitchGame __instance)
        {
            if (LIShipStatus.Instance == null)
                return;

            // Rename all of the nodes to generic names.
            for (int i = 0; i < __instance.Controls.Length; i++)
                __instance.Controls[i].name = $"Node{i}";
        }
    }
}
