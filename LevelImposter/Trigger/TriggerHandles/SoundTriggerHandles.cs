using LevelImposter.Core;

namespace LevelImposter.Trigger;

public class SoundTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "playonce" &&
            signal.TriggerID != "playloop" &&
            signal.TriggerID != "stop")
            return;

        // Get Component
        var triggerSound = signal.TargetObject.GetComponentOrThrow<TriggerSoundPlayer>();

        // Run Sounds
        switch (signal.TriggerID)
        {
            case "playonce":
                triggerSound.Play(false);
                break;
            case "playloop":
                triggerSound.Play(true);
                break;
            case "stop":
                triggerSound.Stop();
                break;
        }
    }
}