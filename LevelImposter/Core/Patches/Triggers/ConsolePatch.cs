using HarmonyLib;
using LevelImposter.Trigger;
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

            // Get Object Data
            var objectData = __instance.gameObject.GetComponent<MapObjectData>();
            if (objectData == null)
                return true;

            // Create Trigger
            bool isClientSide = objectData.Properties.triggerClientSide ?? true;
            TriggerSignal signal = new(__instance.gameObject, "onUse", PlayerControl.LocalPlayer);

            // Fire Trigger
            if (isClientSide)
                TriggerSystem.GetInstance().FireTrigger(signal);
            else
                TriggerSystem.GetInstance().FireTriggerRPC(signal);

            // TODO: Check if should continue?
            return true;
        }
    }
}
