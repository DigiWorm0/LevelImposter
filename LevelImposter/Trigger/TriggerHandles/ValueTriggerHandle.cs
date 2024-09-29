using System;
using LevelImposter.Builders;
using LevelImposter.Core;

namespace LevelImposter.Trigger;

public class ValueTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "setValueTrue" &&
            signal.TriggerID != "setValueFalse" &&
            signal.TriggerID != "toggleValue")
            return;

        // Get Element Data
        var elementData = signal.TargetObject.GetLIData();

        // Get Value
        var valueObj = (BasicBoolValue)ValueBuilder.GetBoolOfID(elementData.ID);
        if (valueObj == null)
            throw new Exception("Value object is not a BasicBoolValue");

        // Run Operation
        var newValue = signal.TriggerID switch
        {
            "setValueTrue" => true,
            "setValueFalse" => false,
            "toggleValue" => !valueObj.GetValue(0),
            _ => throw new Exception("Invalid trigger ID")
        };

        // Set Value
        valueObj.SetValue(newValue, signal);
    }
}