using LevelImposter.Core;

namespace LevelImposter.Trigger;

public class AnimTriggerHandle : ITriggerHandle
{
    private GameObjectCoroutineManager _animManager = new();

    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "playAnim" &&
            signal.TriggerID != "stopAnim" &&
            signal.TriggerID != "pauseAnim")
            return;

        // Get Component
        if (!signal.TargetObject.TryGetComponent(out TriggerAnim animator))
            return;

        // Handle
        if (signal.TriggerID == "playAnim")
            animator.Play(signal);
        else if (signal.TriggerID == "stopAnim")
            animator.Stop();
        else if (signal.TriggerID == "pauseAnim")
            animator.Pause();
    }
}