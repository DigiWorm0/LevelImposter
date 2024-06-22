using UnityEngine;

namespace LevelImposter.Trigger
{
    public class MeetingTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID)
        {
            if (triggerID != "callMeeting")
                return;

            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }
    }
}
