namespace LevelImposter.Trigger.Handles;

public class MeetingTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "callMeeting")
            return;

        PlayerControl.LocalPlayer.CmdReportDeadBody(null);
    }
}