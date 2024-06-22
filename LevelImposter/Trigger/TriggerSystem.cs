using LevelImposter.Core;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelImposter.Trigger
{
    /// <summary>
    /// Object that acts as a trigger to/from another source
    /// </summary>
    public class TriggerSystem
    {
        private const int MAX_STACK_SIZE = 128;

        private List<ITriggerHandle> _triggerHandles = new()
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

            // Propogates triggers to target elements
            new TriggerPropogationHandle()
        };

        private static bool _shouldLog => LIShipStatus.Instance?.CurrentMap?.properties.triggerLogging ?? true;

        public TriggerSystem() => OnCreate();

        /// <summary>
        /// Patch me to add your own custom trigger handles.
        /// Handles should implement <c>ITriggerHandle</c>.
        /// </summary>
        public void OnCreate()
        {
            // ...
        }

        /// <summary>
        /// Finds and fires a trigger on an element ID statically
        /// </summary>
        /// <param name="elementID">Element ID to run trigger on</param>
        /// <param name="triggerID">Trigger ID to fire</param>
        /// <param name="orgin">Orgin player for RPC. Set to <c>null</c> if the trigger should only run on the client</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        /// <returns>TRUE iff the trigger is successful</returns>
        public static void Trigger(Guid elementID, string triggerID, PlayerControl? orgin = null, int stackSize = 0)
        {
            // Get Ship Status
            var shipStatus = LIShipStatus.Instance;
            if (shipStatus == null)
            {
                LILogger.Warn("Ship Status is missing");
                return;
            }

            // Get Object
            var gameObject = shipStatus.MapObjectDB.GetObject(elementID);
            if (gameObject == null)
            {
                LILogger.Warn($"Object with ID {elementID} is missing");
                return;
            }

            // Fire Trigger
            shipStatus.TriggerSystem.FireTrigger(gameObject, triggerID, orgin, stackSize);
        }

        /// <summary>
        /// Finds and fires a trigger on a GameObject statically
        /// </summary>
        /// <param name="gameObject">Object to run trigger on</param>
        /// <param name="triggerID">Trigger ID to fire</param>
        /// <param name="orgin">Orgin player for RPC. Set to <c>null</c> if the trigger should only run on the client</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        /// <returns>TRUE iff the trigger is successful</returns>
        public static void Trigger(GameObject gameObject, string triggerID, PlayerControl? orgin = null, int stackSize = 0)
        {
            // Get Ship Status
            var shipStatus = LIShipStatus.Instance;
            if (shipStatus == null)
            {
                LILogger.Warn("Ship Status is missing");
                return;
            }

            // Fire Trigger
            shipStatus.TriggerSystem.FireTrigger(gameObject, triggerID, orgin, stackSize);
        }

        /// <summary>
        /// Finds and fires a trigger on a GameObject
        /// </summary>
        /// <param name="gameObject">Object to run trigger on</param>
        /// <param name="triggerID">Trigger ID to fire</param>
        /// <param name="orgin">Orgin player for RPC. Set to <c>null</c> if the trigger should only run on the client</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        /// <returns>TRUE iff the trigger is successful</returns>
        private void FireTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin = null, int stackSize = 0)
        {
            // Check if the object was destroyed
            if (gameObject == null)
                return;

            // Run RPC if needed
            if (orgin != null)
            {
                // Get Object Data
                var objectData = gameObject.GetComponent<MapObjectData>();
                if (objectData == null)
                {
                    LILogger.Warn($"{gameObject} is missing LI data");
                    return;
                }

                // Fire RPC
                RPCFireTrigger(orgin, objectData.ID.ToString(), triggerID);

                // Don't run the trigger locally since it will be handled by the RPC
                return;
            }

            // Handle the trigger
            OnTrigger(gameObject, triggerID, orgin, stackSize + 1);
        }

        /// <summary>
        /// Fires a trigger over the network
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
            var shipStatus = LIShipStatus.Instance;
            if (shipStatus == null)
            {
                LILogger.Warn("LIShipStatus is missing");
                return;
            }

            // Parse ID
            Guid elemID;
            if (!Guid.TryParse(elemIDString, out elemID))
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

            // Trigger
            shipStatus.TriggerSystem.OnTrigger(gameObject, triggerID, orgin, 1);
        }

        /// <summary>
        /// Handles a trigger event on the local client
        /// </summary>
        /// <param name="gameObject">GameObject to trigger</param>
        /// <param name="triggerID">Trigger ID to fire</param>
        /// <param name="orgin">Player of orgin</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        private void OnTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin, int stackSize = 0)
        {
            // Infinite Loop
            if (stackSize > MAX_STACK_SIZE)
            {
                LILogger.Warn($"{gameObject.name} >>> {triggerID} detected an infinite trigger loop and aborted");
                LILogger.Info("If you need an infinite loop, enable the loop option on a trigger timer");
                return;
            }

            // Logging
            if (_shouldLog)
            {
                string whitespace = string.Concat(Enumerable.Repeat("| ", stackSize - 1)) + "+ ";
                LILogger.Info($"{whitespace}{gameObject.name} >>> {triggerID} ({orgin?.name})");
            }

            // Handle trigger event
            try
            {
                foreach (ITriggerHandle handle in _triggerHandles)
                    handle.OnTrigger(gameObject, triggerID, orgin, stackSize);
            }
            catch (Exception e)
            {
                LILogger.Error($"Error while handling trigger {gameObject.name} >>> {triggerID}");
                LILogger.Error(e);
            }
        }
    }
}
