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
    public class ActivationPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Console), nameof(Console.Use));
            yield return AccessTools.Method(typeof(SystemConsole), nameof(SystemConsole.Use));
            yield return AccessTools.Method(typeof(DoorConsole), nameof(DoorConsole.Use));
        }

        public static bool Prefix(MonoBehaviour __instance)
        {
            return !LITriggerable.Trigger(__instance.gameObject, "onUse", PlayerControl.LocalPlayer);
        }
    }
}
