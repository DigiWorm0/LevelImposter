using HarmonyLib;
using LevelImposter.Builders;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Calls "onButton" and "onReport" triggers when a meeting is called.
    /// </summary>
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
    public static class MeetingPatch
    {
        public const string BUTTON_TRIGGER_ID = "onButton";
        public const string REPORT_TRIGGER_ID = "onReport";

        public static void Postfix(
            [HarmonyArgument(0)] PlayerControl reporter,
            [HarmonyArgument(1)] NetworkedPlayerInfo target)
        {
            if (LIShipStatus.Instance == null)
                return;

            // Get trigger object
            GameObject? triggerObj = MeetingOptionsBuilder.TriggerObject;
            string triggerID = target == null ? BUTTON_TRIGGER_ID : REPORT_TRIGGER_ID;

            // Call trigger
            if (triggerObj != null)
                LITriggerable.Trigger(triggerObj, triggerID, reporter);
        }
    }
}
