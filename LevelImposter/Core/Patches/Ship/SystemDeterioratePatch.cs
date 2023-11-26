using HarmonyLib;
using System;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Deteriorates all systems that are outside the SystemTypes enum.
    /// </summary>
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
    public static class SystemDeterioratePatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            if (LIShipStatus.Instance == null)
                return;
            if (!AmongUsClient.Instance)
                return;
            if (!PlayerControl.LocalPlayer)
                return;
            if (!AmongUsClient.Instance.AmClient)
                return;

            // Deteriorate all systems that are outside the SystemTypes enum
            foreach (var systemPair in __instance.Systems)
            {
                if (!Enum.IsDefined(typeof(SystemTypes), systemPair.Key))
                    systemPair.value.Deteriorate(Time.fixedDeltaTime);
            }
        }
    }
}
