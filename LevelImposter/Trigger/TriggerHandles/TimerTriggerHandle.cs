using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger;

public class TimerTriggerHandle : ITriggerHandle
{
    private readonly GameObjectCoroutineManager _timerManager = new();

    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "startTimer" &&
            signal.TriggerID != "stopTimer")
            return;

        // Start timer
        if (signal.TriggerID == "startTimer")
            _timerManager.Start(signal.TargetObject, CoTimerTrigger(signal));

        // Stop timer
        else if (signal.TriggerID == "stopTimer") _timerManager.Stop(signal.TargetObject);
    }


    /// <summary>
    ///     Coroutine to run timer trigger. Fires onStart on the start and onFinish on completion.
    /// </summary>
    /// <param name="duration">Duration of the timer in seconds</param>
    [HideFromIl2Cpp]
    private IEnumerator CoTimerTrigger(TriggerSignal signal)
    {
        // Get the object data
        var objectData = signal.TargetObject.GetLIData();

        // Get timer properties
        var duration = objectData.Properties.triggerTime ?? 1;
        var isLoop = objectData.Properties.triggerLoop ?? false;

        // Create Triggers
        var startTrigger = new TriggerSignal(signal.TargetObject, "onStart", signal);
        var endTrigger = new TriggerSignal(signal.TargetObject, "onFinish", signal);

        // Loop Timer
        do
        {
            TriggerSystem.GetInstance().FireTrigger(startTrigger);
            yield return new WaitForSeconds(duration);
            TriggerSystem.GetInstance().FireTrigger(endTrigger);
        } while (isLoop);
    }
}