using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;

namespace LevelImposter.Trigger.Handles;

public class AnimTriggerHandle : ITriggerHandle
{
    private GameObjectCoroutineManager _animManager = new();

    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.EventType != "playAnim" &&
            signal.EventType != "stopAnim" &&
            signal.EventType != "pauseAnim")
            return;

        // Get Component
        if (!signal.TargetObject.TryGetComponent(out TriggerAnim animator))
            return;

        // Handle
        if (signal.EventType == "playAnim")
            animator.Play(signal);
        else if (signal.EventType == "stopAnim")
            animator.Stop();
        else if (signal.EventType == "pauseAnim")
            animator.Pause();
    }
}