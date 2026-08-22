using System.Collections.Generic;
using System.Text.Json;
using LevelImposter.Core.Services.Ship;

namespace LevelImposter.Trigger.Handles;

public class TriggerPropogationHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        // Get the object data
        var element = MapObjectDB.Get(signal.TargetObject);

        // Check if the object has triggers
        var triggers = element?.properties?.triggers;
        if (triggers == null)
            return;

        // Find cooresponding trigger
        foreach (var trigger in triggers)
        {
            // Check if the trigger has the triggerID
            if (trigger.EventType != signal.EventType)
                continue;

            // Check if the trigger should propogate
            if (trigger.TargetID == null || trigger.TargetEventType == null)
                continue;

            // Get Object
            var targetObject = TriggerSystem.FindObject(trigger.TargetID);
            if (targetObject == null)
                continue;

            // Create & Run Trigger
            var newSignal = signal.Propagate(targetObject, trigger.TargetEventType);

            var properties = trigger.Properties ?? new Dictionary<string, JsonElement>();
            foreach (var prop in properties)
                newSignal.Properties[prop.Key] = prop.Value;
            
            TriggerSystem.GetInstance().FireTrigger(newSignal);
            return;
        }
    }
}