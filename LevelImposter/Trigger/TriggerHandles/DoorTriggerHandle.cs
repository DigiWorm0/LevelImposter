using LevelImposter.Core;
using System;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class DoorTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(TriggerSignal signal)
        {
            if (signal.TriggerID == "open")
                SetDoorOpen(signal.TargetObject, true);
            else if (signal.TriggerID == "close")
                SetDoorOpen(signal.TargetObject, false);
        }

        private void SetDoorOpen(GameObject gameObject, bool isOpen)
        {
            // Get the PlainDoor component
            PlainDoor doorComponent = gameObject.GetComponent<PlainDoor>();

            // Check if the object has a PlainDoor component
            if (doorComponent == null)
                throw new Exception($"{gameObject} does not have a PlainDoor component");

            // Set the door state
            doorComponent.SetDoorway(isOpen);
        }

    }
}
