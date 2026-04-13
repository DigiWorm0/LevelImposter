using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger;

public class RandomTriggerHandle : ITriggerHandle
{
    private int _randomOffset;

    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "random")
            return;

        // Get source element
        var element = MapObjectDB.Get(signal.TargetObject);
        if (element == null)
            return;

        // Get a random value
        // Seed is synced across all clients, so the same value is generated on all clients
        var randVal = RandomizerSync.GetRandom(element.id, _randomOffset++);

        // Get the random chance (0 - 1)
        var randomChance = 1.0f / (element.properties.triggerCount ?? 2);

        // Get the trigger index based on the random value (0 - triggerCount)
        var triggerIndex = Mathf.FloorToInt(randVal / randomChance);

        // Get the trigger ID
        var targetID = "onRandom " + (triggerIndex + 1);

        // Create & Fire Trigger
        TriggerSignal newSignal = new(
            signal.TargetObject,
            targetID,
            signal
        );
        TriggerSystem.GetInstance().FireTrigger(newSignal);
    }
}