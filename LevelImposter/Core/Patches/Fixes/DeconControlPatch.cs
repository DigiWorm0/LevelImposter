using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Allows decon controls to bypass physics collisions.
    /// </summary>
    [HarmonyPatch(typeof(DeconControl), nameof(DeconControl.CanUse))]
    public class DeconControlPatch
    {
        public static void Postfix(
            DeconControl __instance,
            [HarmonyArgument(0)] NetworkedPlayerInfo playerInfo,
            [HarmonyArgument(1)] ref bool canUse,
            [HarmonyArgument(2)] ref bool couldUse,
            ref float __result)
        {
            // Custom Maps Only
            if (LIShipStatus.Instance == null)
                return;
            // Ignore if the system is not idle
            if (__instance.System.CurState != DeconSystem.States.Idle)
                return;

            // Check if the player can use the decon
            couldUse = playerInfo.Object.CanMove && !playerInfo.IsDead;
            canUse = (couldUse && __instance.cooldown == 0f);
            __result = float.MaxValue;

            // Check if the player is close enough to use the decon
            if (canUse)
            {
                // Get Adjusted Position
                Vector2 truePosition = playerInfo.Object.GetTruePosition();
                Vector3 position = __instance.transform.position;
                position.y -= 0.1f; // <-- Adjust for player height

                // Compare Distance
                __result = Vector2.Distance(truePosition, position);
                canUse &= (__result <= __instance.UsableDistance);
            }
        }
    }
}
