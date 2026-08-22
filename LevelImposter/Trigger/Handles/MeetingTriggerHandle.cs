namespace LevelImposter.Trigger.Handles;

public class MeetingTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.EventType != "callMeeting")
            return;

        PlayerControl.LocalPlayer.CmdReportDeadBody(null);
    }
}