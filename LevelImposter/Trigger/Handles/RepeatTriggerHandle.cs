namespace LevelImposter.Trigger;

public class RepeatTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "repeat")
            return;

        // Fire Trigger
        for (var i = 1; i <= 8; i++)
        {
            TriggerSignal newSignal = new(signal.TargetObject, $"onRepeat {i}", signal);
            TriggerSystem.GetInstance().FireTrigger(newSignal);
        }
    }
}