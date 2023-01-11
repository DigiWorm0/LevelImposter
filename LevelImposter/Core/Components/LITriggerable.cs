using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Networking.Attributes;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that acts as a trigger to/from another source
    /// </summary>
    public class LITriggerable : MonoBehaviour
    {
        public LITriggerable(IntPtr intPtr) : base(intPtr)
        {
        }

        private const int MAX_STACK_SIZE = 128;
        private static List<LITriggerable> _allTriggers = new List<LITriggerable>();
        public static List<LITriggerable> AllTriggers => _allTriggers;

        private LIElement? _sourceElem = null;
        private Guid? _sourceID => _sourceElem?.id;
        private string _sourceTrigger = "";
        private Guid? _destID = null;
        private string _destTrigger = "";
        private LITriggerable? _destTriggerComp = null;
        private bool _isClientSide => _sourceElem?.properties.triggerClientSide != false;

        [HideFromIl2Cpp]
        public LIElement? SourceElem => _sourceElem;
        public Guid? SourceID => _sourceID;
        public string? SourceTrigger => _sourceTrigger;
        public Guid? DestID => _destID;
        public string? DestTrigger => _destTrigger;
        public LITriggerable? DestTriggerComp => _destTriggerComp;
        public bool IsClientSide => _isClientSide;

        /// <summary>
        /// Finds and fires a trigger on a GameObject
        /// </summary>
        /// <param name="obj">Object to search trigger</param>
        /// <param name="triggerID">Trigger ID to fire</param>
        /// <param name="orgin">Orgin player or null if the trigger should only run on the client</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        public static void Trigger(GameObject obj, string triggerID, PlayerControl? orgin, int stackSize = 0)
        {
            LITriggerable? trigger = AllTriggers.Find(t => t.gameObject == obj && t.SourceTrigger == triggerID);
            if (trigger == null)
                return;

            if (trigger.IsClientSide != false || orgin == null)
                trigger.FireTrigger(orgin, stackSize); // Client-Side
            else
                RPCFireTrigger(orgin, trigger.SourceID.ToString() ?? "", triggerID); // Networked
        }

        /// <summary>
        /// Fires a trigger over the network
        /// </summary>
        /// <param name="orgin">Orgin player</param>
        /// <param name="elemIDString">LIElement ID to fire</param>
        /// <param name="triggerID">Trigger ID to fire</param>
        [MethodRpc((uint)RpcIds.Trigger)]
        public static void RPCFireTrigger(PlayerControl orgin, string elemIDString, string triggerID)
        {
            // Parse ID
            Guid elemID;
            if (!Guid.TryParse(elemIDString, out elemID))
            {
                LILogger.Warn("Triggered element ID is invalid.");
                return;
            }

            // Find Triggerable
            LITriggerable? trigger = AllTriggers.Find(t => t.SourceID == elemID && t.SourceTrigger == triggerID);
            if (trigger == null)
            {
                LILogger.Warn("Triggered element not found");
                return;
            }

            LILogger.Msg($"[RPC] {trigger.gameObject.name} >>> {triggerID} ({orgin.name})");
            trigger.FireTrigger(orgin);
        }

        /// <summary>
        /// Initializes the trigger component
        /// </summary>
        /// <param name="sourceElem">LIElement source</param>
        /// <param name="sourceTrigger">Trigger ID source</param>
        /// <param name="destID">LIElement ID destination</param>
        /// <param name="destTrigger">Trigger ID destination</param>
        [HideFromIl2Cpp]
        public void SetTrigger(LIElement sourceElem, string sourceTrigger, Guid? destID, string destTrigger)
        {
            _sourceElem = sourceElem;
            _sourceTrigger = sourceTrigger;
            _destID = destID;
            _destTrigger = destTrigger;
            if (!_allTriggers.Contains(this))
                _allTriggers.Add(this);
        }

        /// <summary>
        /// Fires the trigger component
        /// </summary>
        /// <param name="orgin">Player of orgin</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        public void FireTrigger(PlayerControl? orgin, int stackSize = 0)
        {
            if (stackSize > MAX_STACK_SIZE)
            {
                LILogger.Warn($"{gameObject.name} >>> {_sourceTrigger} detected an infinite trigger loop and aborted");
                return;
            }
            OnTrigger(orgin, stackSize + 1);
            if (_destTriggerComp != null)
                _destTriggerComp.FireTrigger(orgin, stackSize + 1);
        }

        /// <summary>
        /// Function that fires when the component is triggered
        /// </summary>
        /// <param name="orgin">Player of orgin</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        private void OnTrigger(PlayerControl? orgin, int stackSize = 0)
        {
            LILogger.Info($"{new(' ', stackSize)}{gameObject.name} >>> {_sourceTrigger} ({orgin?.name})");
            switch (_sourceTrigger)
            {
                case "enable":
                    gameObject.SetActive(true);
                    StartSounds();
                    break;
                case "disable":
                    gameObject.SetActive(false);
                    StopSounds();
                    break;
                case "show":
                    gameObject.SetActive(true);
                    break;
                case "hide":
                    gameObject.SetActive(false);
                    break;
                case "repeat":
                    for (int i = 0; i < 8; i++)
                        Trigger(gameObject, "onRepeat " + (i + 1), orgin, stackSize + 1);
                    break;
                case "startTimer":
                    StartCoroutine(CoTimerTrigger(orgin, stackSize).WrapToIl2Cpp());
                    break;
                case "open":
                    SetDoorOpen(true);
                    break;
                case "close":
                    SetDoorOpen(false);
                    break;

            }
        }

        /// <summary>
        /// Stops any ambient sound attatched to object
        /// </summary>
        private void StopSounds()
        {
            AmbientSoundPlayer? ambientSound = GetComponent<AmbientSoundPlayer>();
            if (ambientSound == null)
                return;
            ambientSound.OnDestroy();
        }
        
        /// <summary>
        /// Starts any ambient sound attatched to object
        /// </summary>
        private void StartSounds()
        {
            AmbientSoundPlayer? ambientSound = GetComponent<AmbientSoundPlayer>();
            if (ambientSound == null)
                return;
            ambientSound.Start();
        }

        /// <summary>
        /// Coroutine to run timer trigger. Fires onStart on the start and onFinish on completion.
        /// </summary>
        /// <param name="orgin">Player of orgin</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        [HideFromIl2Cpp]
        private IEnumerator CoTimerTrigger(PlayerControl? orgin, int stackSize = 1)
        {
            Trigger(gameObject, "onStart", orgin, stackSize);
            float duration = _sourceElem?.properties.triggerTime ?? 1;
            yield return new WaitForSeconds(duration);
            Trigger(gameObject, "onFinish", orgin, stackSize);
        }

        /// <summary>
        /// Sets the open state of the doorComponent on the gameObject
        /// </summary>
        /// <param name="isOpen">TRUE if the door should be open</param>
        private void SetDoorOpen(bool isOpen)
        {
            PlainDoor doorComponent = gameObject.GetComponent<PlainDoor>();
            doorComponent.SetDoorway(isOpen);
        }

        public void Start()
        {
            _destTriggerComp = AllTriggers.Find(t => _destID == t._sourceID && _destTrigger == t._sourceTrigger);
        }
        public void OnDestroy()
        {
            _allTriggers.Remove(this);
            _sourceElem = null;
            _destID = null;
            _destTriggerComp = null;
        }
    }
}
