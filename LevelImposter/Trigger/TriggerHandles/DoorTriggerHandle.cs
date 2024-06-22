using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class DoorTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID)
        {
            if (triggerID == "open")
                SetDoorOpen(gameObject, true);
            else if (triggerID == "close")
                SetDoorOpen(gameObject, false);
        }

        private void SetDoorOpen(GameObject gameObject, bool isOpen)
        {
            // Get the PlainDoor component
            PlainDoor doorComponent = gameObject.GetComponent<PlainDoor>();

            // Check if the object has a PlainDoor component
            if (doorComponent == null)
                LILogger.Warn($"{gameObject} does not have a PlainDoor component");

            // Set the door state
            doorComponent?.SetDoorway(isOpen);
        }

    }
}
