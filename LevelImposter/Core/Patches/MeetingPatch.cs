using HarmonyLib;
using UnityEngine;
using LevelImposter.Builders;
using InnerNet;

namespace LevelImposter.Core
{
    /*
     *      Calls meeting triggers
     */
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
    public static class MeetingPatch
    {
        public const string BUTTON_TRIGGER_ID = "onButton";
        public const string REPORT_TRIGGER_ID = "onReport";

        public static void Postfix([HarmonyArgument(0)] PlayerControl reporter, [HarmonyArgument(1)] GameData.PlayerInfo target)
        {
            GameObject? triggerObj = MeetingBuilder.TriggerObject;
            string triggerID = target == null ? BUTTON_TRIGGER_ID : REPORT_TRIGGER_ID;
            if (triggerObj != null)
                LITriggerable.Trigger(triggerObj, triggerID, reporter);
        }
    }
}
