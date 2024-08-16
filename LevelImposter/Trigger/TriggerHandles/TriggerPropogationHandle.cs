using LevelImposter.Core;
using System;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class TriggerPropogationHandle : ITriggerHandle
    {
        public void OnTrigger(TriggerSignal signal)
        {
            // Get the object data
            var objectData = signal.TargetObject.GetLIData();

            // Check if the object has triggers
            if (objectData.Properties.triggers == null)
                return;

            // Find cooresponding trigger
            foreach (var trigger in objectData.Properties.triggers)
            {
                // Check if the trigger has the triggerID
                if (trigger.id != signal.TriggerID)
                    continue;

                // Check if the trigger should propogate
                if (trigger.elemID == null || trigger.triggerID == null)
                    continue;

                // Get Object
                var targetObject = TriggerSystem.FindObject(trigger.elemID);
                if (targetObject == null)
                    continue;

                // Create & Run Trigger
                TriggerSignal newSignal = new(
                    targetObject,
                    trigger.triggerID,
                    signal
                );
                TriggerSystem.GetInstance().FireTrigger(newSignal);
                return;
            }
        }
    }
}
