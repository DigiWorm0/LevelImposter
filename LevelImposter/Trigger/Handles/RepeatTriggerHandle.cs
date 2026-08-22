namespace LevelImposter.Trigger.Handles;

public class RepeatTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.EventType != "repeat")
            return;

        // Fire Trigger
        for (var i = 1; i <= 8; i++)
        {
            var newSignal = signal.Propagate(signal.TargetObject, $"onRepeat {i}");
            TriggerSystem.GetInstance().FireTrigger(newSignal);
        }
    }
}