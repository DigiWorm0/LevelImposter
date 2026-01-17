using System;
using LevelImposter.Trigger;

namespace LevelImposter.Core;

public class BasicBoolValue(Guid id, bool value) : IBoolValue
{
    private bool _value = value;

    public bool GetValue(int depth)
    {
        return _value;
    }

    public void SetValue(bool value, TriggerSignal sourceSignal)
    {
        _value = value;
        OnValueChange(sourceSignal);
    }

    private void OnValueChange(TriggerSignal sourceSignal)
    {
        // Get Target Object
        var targetObj = LIShipStatus.MapObjectDB.GetObject(id);
        if (targetObj == null)
            return;

        // Fire Trigger
        var signal = new TriggerSignal(targetObj, "onChange", sourceSignal);
        TriggerSystem.GetInstance().FireTrigger(signal);
    }
}