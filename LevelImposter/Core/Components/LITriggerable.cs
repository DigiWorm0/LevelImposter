using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private static readonly string[] CLIENT_ONLY_TRIGGERS = new string[]
        {
            "startOxygen",
            "startReactor",
            "startComms",
            "startLights",
            "endOxygen",
            "endReactor",
            "endComms",
            "endLights"
        };
        private static List<LITriggerable> _allTriggers = new List<LITriggerable>();
        private static bool _shouldLog => LIShipStatus.Instance?.CurrentMap?.properties.triggerLogging ?? true;

        private LIElement? _sourceElem = null;
        private Guid? _sourceID => _sourceElem?.id;
        private string _sourceTrigger = "";
        private Guid? _destID = null;
        private string? _destTrigger = "";
        private LITriggerable? _destTriggerComp = null;
        private bool _isClientSide => _sourceElem?.properties.triggerClientSide != false;
        private int _randomOffset = 0;
        private float _randomChance = 0.5f;

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
        /// <param name="orgin">Orgin player for RPC. Set to <c>null</c> if the trigger should only run on the client</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        /// <returns>TRUE iff the trigger is successful</returns>
        public static bool Trigger(GameObject? obj, string triggerID, PlayerControl? orgin, int stackSize = 0)
        {
            if (obj == null)
                return false;

            LITriggerable? trigger = _allTriggers.Find(t => t.gameObject == obj && t.SourceTrigger == triggerID);
            if (trigger == null)
                return false;

            if (trigger.IsClientSide != false ||
                orgin == null ||
                CLIENT_ONLY_TRIGGERS.Contains(triggerID))
            {
                trigger.FireTrigger(orgin, stackSize); // Client-Side
            }
            else
            {
                RPCFireTrigger(orgin, trigger.SourceID.ToString() ?? "", triggerID); // Networked
            }
            return true;
        }

        /// <summary>
        /// Fires a trigger over the network
        /// </summary>
        /// <param name="orgin">Orgin player</param>
        /// <param name="elemIDString">LIElement ID to fire</param>
        /// <param name="triggerID">Trigger ID to fire</param>
        [MethodRpc((uint)LIRpc.FireTrigger)]
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
            LITriggerable? trigger = _allTriggers.Find(t => t.SourceID == elemID && t.SourceTrigger == triggerID);
            if (trigger == null)
            {
                LILogger.Warn("Triggered element not found");
                return;
            }

            if (_shouldLog)
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
        public void SetTrigger(LIElement sourceElem, string sourceTrigger, Guid? destID, string? destTrigger)
        {
            _sourceElem = sourceElem;
            _sourceTrigger = sourceTrigger;
            _destID = destID;
            _destTrigger = destTrigger;
            if (sourceTrigger == "random")
                _randomChance = 1.0f / (sourceElem.properties.triggerCount ?? 2);
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
            // Logging
            if (_shouldLog)
            {
                string whitespace = string.Concat(Enumerable.Repeat("| ", stackSize - 1)) + "+ ";
                LILogger.Info($"{whitespace}{gameObject.name} >>> {_sourceTrigger} ({orgin?.name})");
            }

            // Client Only
            bool isOnClient = orgin == null || orgin == PlayerControl.LocalPlayer;
            if (CLIENT_ONLY_TRIGGERS.Contains(_sourceTrigger) && !isOnClient)
                return;

            // Run Trigger
            switch (_sourceTrigger)
            {
                // Generic
                case "enable":
                    StartComponents();
                    break;
                case "disable":
                    StopComponents();
                    break;
                case "show":
                    gameObject.SetActive(true);
                    StartComponents();
                    break;
                case "hide":
                    gameObject.SetActive(false);
                    StopComponents();
                    break;

                // Repeat
                case "repeat":
                    for (int i = 0; i < 8; i++)
                        Trigger(gameObject, "onRepeat " + (i + 1), orgin, stackSize + 1);
                    break;

                // Random
                case "random":
                    if (_sourceID == null)
                        return;
                    float randVal = RandomizerSync.GetRandom((Guid)_sourceID, _randomOffset);
                    int triggerIndex = Mathf.FloorToInt(randVal / _randomChance);
                    string triggerID = "onRandom " + (triggerIndex + 1);
                    Trigger(gameObject, triggerID, orgin, stackSize + 1);
                    _randomOffset++;
                    break;

                // Timer
                case "startTimer":
                    StartCoroutine(CoTimerTrigger(orgin, stackSize).WrapToIl2Cpp());
                    break;

                // Door
                case "open":
                    SetDoorOpen(true);
                    break;
                case "close":
                    SetDoorOpen(false);
                    break;

                // Meeting
                case "callMeeting":
                    PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                    break;

                // Playback
                case "playonce":
                    StartComponents(false);
                    break;
                case "playloop":
                    StartComponents(true);
                    break;
                case "stop":
                    StopComponents();
                    break;

                // Sabotage
                case "startOxygen":
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 128);
                    break;
                case "startLights":
                    byte switchBits = 4;
                    for (int i = 0; i < 5; i++)
                        if (BoolRange.Next(0.5f))
                            switchBits |= (byte)(1 << i);
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)(switchBits | 128));
                    break;
                case "startReactor":
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 128);
                    break;
                case "startComms":
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 128);
                    break;
                case "startMixup":
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 1);
                    break;
                case "endOxygen":
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 16);
                    break;
                case "endLights":
                    var lights = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)((lights.ExpectedSwitches ^ lights.ActualSwitches) | 128));
                    break;
                case "endReactor":
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16);
                    break;
                case "endComms":
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 0);
                    break;
                case "endMixup":
                    if (!ShipStatus.Instance.Systems.ContainsKey(SystemTypes.MushroomMixupSabotage))
                        return;
                    var mixup = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage].Cast<MushroomMixupSabotageSystem>();
                    mixup.currentSecondsUntilHeal = 0.01f;
                    mixup.IsDirty = true;
                    // TODO: Transmit to other clients
                    break;
            }
        }

        /// <summary>
        /// Stops any components attatched to object
        /// </summary>
        private void StopComponents()
        {
            AmbientSoundPlayer? ambientSound = GetComponent<AmbientSoundPlayer>();
            if (ambientSound != null)
                ambientSound.enabled = false;
            GIFAnimator? gifAnimator = GetComponent<GIFAnimator>();
            if (gifAnimator != null)
                gifAnimator.Stop();
            TriggerConsole triggerConsole = GetComponent<TriggerConsole>();
            if (triggerConsole != null)
                triggerConsole.SetEnabled(false);
            SystemConsole sysConsole = GetComponent<SystemConsole>();
            if (sysConsole != null)
                sysConsole.enabled = false;
            TriggerSoundPlayer? triggerSound = GetComponent<TriggerSoundPlayer>();
            if (triggerSound != null)
                triggerSound.Stop();
        }

        /// <summary>
        /// Starts any components attatched to object
        /// </summary>
        private void StartComponents(bool loop = true)
        {
            AmbientSoundPlayer? ambientSound = GetComponent<AmbientSoundPlayer>();
            if (ambientSound != null)
                ambientSound.enabled = true;
            GIFAnimator? gifAnimator = GetComponent<GIFAnimator>();
            if (gifAnimator != null)
                gifAnimator.Play();
            TriggerConsole? triggerConsole = GetComponent<TriggerConsole>();
            if (triggerConsole != null)
                triggerConsole.SetEnabled(true);
            SystemConsole? sysConsole = GetComponent<SystemConsole>();
            if (sysConsole != null)
                sysConsole.enabled = true;
            TriggerSoundPlayer? triggerSound = GetComponent<TriggerSoundPlayer>();
            if (triggerSound != null)
                triggerSound.Play(loop);
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
            if (doorComponent == null)
                LILogger.Warn($"{name} does not have a PlainDoor component");
            doorComponent?.SetDoorway(isOpen);
        }
        public void Start()
        {
            _destTriggerComp = _allTriggers.Find(t => _destID == t._sourceID && _destTrigger == t._sourceTrigger);
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
