using LevelImposter.Builders.Util;
using LevelImposter.Core.Services.Ship;

namespace LevelImposter.Trigger.Handles;

public class GateTriggerHandle : ITriggerHandle
{
    private const string ON_TRUE = "onTrue";
    private const string ON_FALSE = "onFalse";

    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "triggerGate")
            return;

        // Get Value
        var element = MapObjectDB.Get(signal.TargetObject);
        var valueObj = ValueBuilder.GetBoolOfID(element?.properties.triggerGateValueID);
        var value = valueObj.GetValue();

        // Fire Trigger
        var triggerID = value ? ON_TRUE : ON_FALSE;
        TriggerSignal newSignal = new(signal.TargetObject, triggerID, signal);
        TriggerSystem.GetInstance().FireTrigger(newSignal);
    }
}