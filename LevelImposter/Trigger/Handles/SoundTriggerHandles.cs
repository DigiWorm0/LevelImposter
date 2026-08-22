using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;

namespace LevelImposter.Trigger.Handles;

public class SoundTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.EventType != "playonce" &&
            signal.EventType != "playloop" &&
            signal.EventType != "stop")
            return;

        // Get Component
        var triggerSound = signal.TargetObject.GetComponentOrThrow<TriggerSoundPlayer>();

        // Run Sounds
        switch (signal.EventType)
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