using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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

            // Get IUsable
            var usable = __instance.TryCast<IUsable>();
            if (usable == null)
                return true;

            // Check if the player can use the console
            usable.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out _);
            if (!canUse)
                return true;

            // Update Last Console
            MinigamePatch.LastConsole = __instance.gameObject;

            // Trigger "onUse" event
            return !LITriggerable.Trigger(__instance.gameObject, "onUse", PlayerControl.LocalPlayer);
        }
    }
}
