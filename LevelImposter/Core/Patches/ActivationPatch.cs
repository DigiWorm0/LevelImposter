using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace LevelImposter.Core
{
    [HarmonyPatch]
    public class ActivationPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Console), nameof(Console.Use));
            yield return AccessTools.Method(typeof(SystemConsole), nameof(SystemConsole.Use));
        }

        public static void Postfix(MonoBehaviour __instance)
        {
            LITriggerable[] triggers = __instance.GetComponents<LITriggerable>();
            LITriggerable trigger = Array.Find(triggers, (LITriggerable t) => t.id == "onUse");
            if (trigger != null)
                trigger.Trigger();
        }
    }
}
