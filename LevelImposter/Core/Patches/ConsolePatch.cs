using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace LevelImposter.Core
{
    /// <summary>
    /// Fires "onUse" triggers on a variety of console objects
    /// </summary>
    [HarmonyPatch]
    public class ConsolePatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Console), nameof(Console.Use));
            yield return AccessTools.Method(typeof(SystemConsole), nameof(SystemConsole.Use));
            yield return AccessTools.Method(typeof(DoorConsole), nameof(DoorConsole.Use));
            yield return AccessTools.Method(typeof(MapConsole), nameof(MapConsole.Use));
        }

        public static bool Prefix(MonoBehaviour __instance)
        {
            if (LIShipStatus.Instance == null)
                return true;

            // Update Last Console
            MinigamePatch.LastConsole = __instance.gameObject;
            // Trigger "onUse" event
            return !LITriggerable.Trigger(__instance.gameObject, "onUse", PlayerControl.LocalPlayer);
        }
    }

    /// <summary>
    /// Replaces the "CheckWalls" check from
    /// a Shadow Mask to an Object Mask
    /// </summary>
    /*
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class HorsePatch
    {
        public static void Postfix(
            Console __instance,
            [HarmonyArgument(0)]GameData.PlayerInfo pc,
            [HarmonyArgument(1)] ref bool canUse)
        {
            if (LIShipStatus.Instance == null)
                return;
            if (!__instance.checkWalls)
                return;

            // Check Wall Collision
            Vector2 truePosition = pc.Object.GetTruePosition();
            Vector3 position = __instance.transform.position;
            canUse &= !PhysicsHelpers.AnythingBetween(
                truePosition,
                position,
                Constants.ShipAndAllObjectsMask,
                false
            );
        }
    }*/
}
