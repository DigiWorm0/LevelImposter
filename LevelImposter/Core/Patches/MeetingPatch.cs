using HarmonyLib;
using UnityEngine;
using LevelImposter.Builders;

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
            if (LIShipStatus.Instance == null)
                return;
            GameObject? triggerObj = MeetingOptionsBuilder.TriggerObject;
            string triggerID = target == null ? BUTTON_TRIGGER_ID : REPORT_TRIGGER_ID;
            if (triggerObj != null)
                LITriggerable.Trigger(triggerObj, triggerID, reporter);
        }
    }
    /*
     *      Fixes PoolablePlayer running Awake too early
     *      when making meeting prefabs
     */
    [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.Initialize))]
    public static class MeetingOverlayPatch
    {
        public static void Prefix(MeetingCalledAnimation __instance)
        {
            if (LIShipStatus.Instance == null)
                return;
            __instance.playerParts.InitBody();
        }
    }
}
