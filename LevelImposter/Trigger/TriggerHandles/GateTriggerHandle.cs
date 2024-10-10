using LevelImposter.Builders;
using LevelImposter.Core;

namespace LevelImposter.Trigger;

public class GateTriggerHandle : ITriggerHandle
{
    private const string ON_TRUE = "onTrue";
    private const string ON_FALSE = "onFalse";

    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "triggerGate")
            return;

        // Get Value
        var elementData = signal.TargetObject.GetLIData();
        var valueObj = ValueBuilder.GetBoolOfID(elementData.Properties.triggerGateValueID);
        var value = valueObj.GetValue(0);

        // Fire Trigger
        var triggerID = value ? ON_TRUE : ON_FALSE;
        TriggerSignal newSignal = new(signal.TargetObject, triggerID, signal);
        TriggerSystem.GetInstance().FireTrigger(newSignal);
    }
}