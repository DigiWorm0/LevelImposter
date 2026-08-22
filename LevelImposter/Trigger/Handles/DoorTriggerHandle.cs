using System;
using UnityEngine;

namespace LevelImposter.Trigger.Handles;

public class DoorTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.EventType == "open")
            SetDoorOpen(signal.TargetObject, true);
        else if (signal.EventType == "close")
            SetDoorOpen(signal.TargetObject, false);
    }

    private void SetDoorOpen(GameObject gameObject, bool isOpen)
    {
        // Get the PlainDoor component
        var doorComponent = gameObject.GetComponent<PlainDoor>();

        // Check if the object has a PlainDoor component
        if (doorComponent == null)
            throw new Exception($"{gameObject} does not have a PlainDoor component");

        // Set the door state
        doorComponent.SetDoorway(isOpen);
    }
}