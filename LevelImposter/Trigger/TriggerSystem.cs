using System;
using System.Collections.Generic;
using System.Linq;
using LevelImposter.Core;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LevelImposter.Trigger;

/// <summary>
///     Object that acts as a trigger to/from another source
/// </summary>
public class TriggerSystem
{
    private const int MAX_STACK_SIZE = 128;

    private readonly List<ITriggerHandle> _triggerHandles = new()
    {
        // Handles trigger effects
        new DeathTriggerHandle(),
        new DoorTriggerHandle(),
        new MeetingTriggerHandle(),
        new RandomTriggerHandle(),
        new RepeatTriggerHandle(),
        new SabTriggerHandle(),
        new ShowHideTriggerHandle(),
        new SoundTriggerHandle(),
        new TeleportTriggerHandle(),
        new TimerTriggerHandle(),
        new AnimTriggerHandle(),

        // Propogates triggers to target elements
        new TriggerPropogationHandle()
    };

    public TriggerSystem()
    {
        OnCreate();
    }

    private static bool _shouldLog =>
        LIShipStatus.GetInstanceOrNull()?.CurrentMap?.properties.triggerLogging ?? true;

    /// <summary>
    ///     Patch me to add your own custom trigger handles.
    ///     Handles should implement <c>ITriggerHandle</c>.
    /// </summary>
    public void OnCreate()
    {
        // ...
    }

    /// <summary>
    ///     Gets the global instance of the trigger system
    /// </summary>
    /// <returns>The global instance of the trigger system</returns>
    /// <exception cref="MissingShipException">If LIShipStatus is missing</exception>
    public static TriggerSystem GetInstance()
    {
        return LIShipStatus.GetInstance().TriggerSystem;
    }

    /// <summary>
    ///     Finds an object by its ID
    /// </summary>
    /// <param name="objectID">ID of the object</param>
    /// <returns>The cooresponding GameObject</returns>
    /// <exception cref="Exception"></exception>
    public static GameObject? FindObject(Guid? objectID)
    {
        if (objectID == null)
            return null;

        // Get Object
        return LIShipStatus.GetInstance().MapObjectDB.GetObject((Guid)objectID);
    }

    /// <summary>
    ///     Fires a trigger over RPC
    /// </summary>
    /// <param name="signal"></param>
    public void FireTriggerRPC(TriggerSignal signal)
    {
        // Check Player
        if (signal.SourcePlayer == null)
            throw new Exception("Missing PlayerControl on TriggerSignal");

        // Get Object Data
        var objectData = signal.TargetObject.GetComponent<MapObjectData>();
        if (objectData == null)
            throw new Exception($"{signal.TargetObject} is missing LI data");

        // Fire Trigger over RPC
        RPCFireTrigger(
            signal.SourcePlayer,
            objectData.ID.ToString(),
            signal.TriggerID
        );
    }

    /// <summary>
    ///     Fires a trigger over the network
    /// </summary>
    /// <param name="orgin">Orgin player</param>
    /// <param name="elemIDString">LIElement ID to fire</param>
    /// <param name="triggerID">Trigger ID to fire</param>
    [MethodRpc((uint)LIRpc.FireTrigger)]
    private static void RPCFireTrigger(PlayerControl orgin, string elemIDString, string triggerID)
    {
        // Log
        if (_shouldLog)
            LILogger.Msg($"[RPC] {elemIDString} >>> {triggerID} ({orgin.name})");

        // Get Ship Status
        var shipStatus = LIShipStatus.GetInstance();

        // Parse ID
        if (!Guid.TryParse(elemIDString, out var elemID))
        {
            LILogger.Warn("RPC triggered element ID is invalid.");
            return;
        }

        // Find cooresponding object
        var gameObject = shipStatus.MapObjectDB.GetObject(elemID);
        if (gameObject == null)
        {
            LILogger.Warn($"RPC object with ID {elemID} is missing");
            return;
        }

        // Create & Fire Trigger
        TriggerSignal signal = new(gameObject, triggerID, orgin);
        shipStatus.TriggerSystem.FireTrigger(signal);
    }

    /// <summary>
    ///     Handles a trigger event on the local client
    /// </summary>
    /// <param name="signal">Signal data object</param>
    public void FireTrigger(TriggerSignal signal)
    {
        // Object Name (For Logging)
        var objectName = signal.TargetObject == null ? "[null]" : signal.TargetObject.name;
        var playerName = signal.SourcePlayer == null ? "null" : signal.SourcePlayer.name;

        // Infinite Loop
        if (signal.StackSize > MAX_STACK_SIZE)
        {
            LILogger.Warn(
                $"{objectName} >>> {signal.TriggerID} detected an infinite trigger loop and aborted");
            LILogger.Info("If you need an infinite loop, enable the loop option on a trigger timer");
            return;
        }

        // Logging
        if (_shouldLog)
        {
            var whitespace = string.Concat(Enumerable.Repeat("| ", signal.StackSize - 1)) + "+ ";
            LILogger.Info(
                $"{whitespace}{objectName} >>> {signal.TriggerID} ({playerName})");
        }

        // Check Validity
        if (signal.TargetObject == null)
            return;

        // Handle trigger event
        try
        {
            foreach (var handle in _triggerHandles)
                handle.OnTrigger(signal);
        }
        catch (Exception e)
        {
            LILogger.Error($"Error while handling trigger {objectName} >>> {signal.TriggerID}");
            LILogger.Error(e);
        }
    }
}