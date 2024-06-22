using LevelImposter.Core;
using System;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class TriggerPropogationHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin, int stackSize = 0)
        {
            // Get the object data
            var objectData = gameObject.GetComponent<MapObjectData>();
            if (objectData == null)
            {
                LILogger.Warn($"{gameObject} is missing LI data");
                return;
            }

            // Check if the object has triggers
            if (objectData.Properties.triggers == null)
                return;

            // Find cooresponding trigger
            foreach (var trigger in objectData.Properties.triggers)
            {
                // Check if the trigger has the triggerID
                if (trigger.id != triggerID)
                    continue;

                // Check if the trigger should propogate
                if (trigger.elemID == null || trigger.triggerID == null)
                    continue;

                // Run trigger
                TriggerSystem.Trigger((Guid)trigger.elemID, trigger.triggerID ?? "", orgin, stackSize);
                return;
            }
        }
    }
}
