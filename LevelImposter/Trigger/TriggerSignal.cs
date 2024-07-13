using LevelImposter.Core;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelImposter.Trigger
{
    /// <summary>
    /// Trigger signal
    /// </summary>
    public class TriggerSignal
    {
        /// <summary>
        /// Constructs a trigger signal from a player
        /// </summary>
        /// <param name="targetObject">The object to trigger</param>
        /// <param name="triggerID">The trigger ID</param>
        /// <param name="sourcePlayer">The player that triggered the object</param>
        public TriggerSignal(GameObject targetObject, string triggerID, PlayerControl? sourcePlayer)
        {
            TargetObject = targetObject;
            TriggerID = triggerID;
            StackSize = 1;
            SourcePlayer = sourcePlayer;
        }

        /// <summary>
        /// Constructs a trigger signal that propogated from another trigger
        /// </summary>
        /// <param name="targetObject">The object to trigger</param>
        /// <param name="triggerID">The trigger ID</param>
        /// <param name="sourceTrigger">The original trigger that ran this trigger</param>
        public TriggerSignal(GameObject targetObject, string triggerID, TriggerSignal? sourceTrigger)
        {
            TargetObject = targetObject;
            TriggerID = triggerID;
            StackSize = (sourceTrigger?.StackSize ?? 0) + 1;
            SourcePlayer = sourceTrigger?.SourcePlayer;
            SourceTrigger = sourceTrigger;
        }

        public GameObject TargetObject { get; private set; }
        public string TriggerID { get; private set; }
        public int StackSize {  get; private set; }

        // Options
        public PlayerControl? SourcePlayer { get; private set; }
        public TriggerSignal? SourceTrigger { get; private set; }
    }
}
