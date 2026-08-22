using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace LevelImposter.Trigger;

/// <summary>
///     Trigger signal
/// </summary>
public class TriggerSignal
{
    protected TriggerSignal(
        GameObject targetObject,
        string eventType,
        int stackSize,
        PlayerControl? sourcePlayer,
        TriggerSignal? sourceTrigger)
    {
        TargetObject = targetObject;
        EventType = eventType;
        StackSize = stackSize;
        SourcePlayer = sourcePlayer;
        SourceTrigger = sourceTrigger;
    }

    public GameObject TargetObject { get; private set; }
    public string EventType { get; private set; }
    public int StackSize { get; }

    // Options
    public PlayerControl? SourcePlayer { get; }
    public TriggerSignal? SourceTrigger { get; private set; }
    public Dictionary<string, JsonElement> Properties { get; } = new();

    /// <summary>
    ///     Creates a new trigger signal based on a specific event.
    /// </summary>
    /// <param name="targetObject">The object to target</param>
    /// <param name="eventType">The type of event to trigger</param>
    /// <param name="sourcePlayer">The player who caused the event to occur</param>
    /// <returns></returns>
    public static TriggerSignal NewEvent(GameObject targetObject, string eventType, PlayerControl? sourcePlayer)
    {
        return new TriggerSignal(
            targetObject,
            eventType,
            1,
            sourcePlayer,
            null);
    }

    /// <summary>
    ///     Creates a new TriggerSignal that propagated from this signal.
    /// </summary>
    /// <param name="newObject">The new object to target</param>
    /// <param name="eventType">The new event to trigger</param>
    /// <returns>A new TriggerSignal that has this signal as its source.</returns>
    public TriggerSignal Propagate(GameObject newObject, string eventType)
    {
        return new TriggerSignal(
            newObject,
            eventType,
            StackSize + 1,
            SourcePlayer,
            this);
    }
}