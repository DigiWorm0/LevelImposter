using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core.Services.Ship;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Trigger.Handles;

public class TimerTriggerHandle : ITriggerHandle
{
    private readonly GameObjectCoroutineManager _timerManager = new();

    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.EventType != "startTimer" &&
            signal.EventType != "stopTimer")
            return;

        // Start timer
        if (signal.EventType == "startTimer")
            _timerManager.Start(signal.TargetObject, CoTimerTrigger(signal));

        // Stop timer
        else if (signal.EventType == "stopTimer") _timerManager.Stop(signal.TargetObject);
    }


    /// <summary>
    ///     Coroutine to run timer trigger. Fires onStart on the start and onFinish on completion.
    /// </summary>
    /// <param name="signal">The originating trigger signal</param>
    [HideFromIl2Cpp]
    private IEnumerator CoTimerTrigger(TriggerSignal signal)
    {
        // Get the object data
        var element = MapObjectDB.Get(signal.TargetObject);

        // Get timer properties
        var duration = element?.properties.triggerTime ?? 1;
        var isLoop = element?.properties.triggerLoop ?? false;

        // Create Triggers
        var startTrigger = signal.Propagate(signal.TargetObject, "onStart");
        var endTrigger = signal.Propagate(signal.TargetObject, "onFinish");

        // Loop Timer
        do
        {
            TriggerSystem.GetInstance().FireTrigger(startTrigger);
            yield return new WaitForSeconds(duration);
            TriggerSystem.GetInstance().FireTrigger(endTrigger);
        } while (isLoop);
    }
}