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
        private static readonly string[] COMPONENT_TYPES = new string[]
        {
            nameof(AmbientSoundPlayer),
            nameof(TriggerConsole),
            nameof(SystemConsole),
            nameof(MapConsole),
            nameof(LITeleporter)
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

        // Randomizer
        private int _randomOffset = 0;
        private float _randomChance => 1.0f / (_sourceElem?.properties.triggerCount ?? 2);

        // Timer
        private Coroutine? _timerCoroutine = null;

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
                    SetComponentsEnabled(true);
                    break;
                case "disable":
                    SetComponentsEnabled(false);
                    break;
                case "toggle":
                    SetComponentsEnabled();
                    break;
                case "show":
                    gameObject.SetActive(true);
                    SetComponentsEnabled(true);
                    break;
                case "hide":
                    gameObject.SetActive(false);
                    SetComponentsEnabled(false);
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
                    if (_timerCoroutine != null)
                        StopCoroutine(_timerCoroutine);
                    _timerCoroutine = StartCoroutine(CoTimerTrigger(orgin, stackSize + 1).WrapToIl2Cpp());
                    break;
                case "stopTimer":
                    LITriggerable[] triggers = GetComponents<LITriggerable>();
                    foreach (LITriggerable trigger in triggers)
                        if (trigger.SourceTrigger == "startTimer")
                            trigger.StopTimer();
                    break;

                // Death
                case "killArea":
                    SetComponentsEnabled(true);
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
                    SetComponentsEnabled(true, false);
                    break;
                case "playloop":
                    SetComponentsEnabled(true, true);
                    break;
                case "stop":
                    SetComponentsEnabled(false);
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

                // Teleport
                case "teleportonce":
                    GetComponent<LITeleporter>()?.TeleportOnce();
                    break;
            }
        }

        /// <summary>
        /// Enables or disables specific components on the object
        /// </summary>
        /// <param name="isEnabled"><c>true</c> if the components should be enabled. <c>false</c> otherwise</param>
        /// <param name="isLooped"><c>true</c> if GIF animations or sounds should loop. <c>false</c> otherwise</param>
        private void SetComponentsEnabled(bool? isEnabled = null, bool? isLooped = null)
        {
            // Trigger Sounds
            TriggerSoundPlayer? triggerSound = GetComponent<TriggerSoundPlayer>();
            if (triggerSound != null)
            {
                if (isEnabled ?? !triggerSound.IsPlaying)
                    triggerSound.Play(isLooped ?? false);
                else
                    triggerSound.Stop();
            }

            // Animation
            GIFAnimator? gifAnimator = GetComponent<GIFAnimator>();
            if (gifAnimator != null)
            {
                if (isEnabled ?? !gifAnimator.IsAnimating)
                    gifAnimator.Play();
                else
                    gifAnimator.Stop();
            }

            // Ambient Sounds
            AmbientSoundPlayer? ambientSound = GetComponent<AmbientSoundPlayer>();
            if (ambientSound != null)
                ambientSound.enabled = isEnabled ?? !ambientSound.enabled;

            // Trigger Console
            TriggerConsole triggerConsole = GetComponent<TriggerConsole>();
            if (triggerConsole != null)
                triggerConsole.enabled = isEnabled ?? !triggerConsole.enabled;

            // System Console
            SystemConsole sysConsole = GetComponent<SystemConsole>();
            if (sysConsole != null)
                sysConsole.enabled = isEnabled ?? !sysConsole.enabled;

            // Map Console
            MapConsole? mapConsole = GetComponent<MapConsole>();
            if (mapConsole != null)
                mapConsole.enabled = isEnabled ?? !mapConsole.enabled;

            // Teleporter
            LITeleporter? teleporter = GetComponent<LITeleporter>();
            if (teleporter != null)
                teleporter.enabled = isEnabled ?? !teleporter.enabled;

            // Kill Area
            LIDeathArea? deathArea = GetComponent<LIDeathArea>();
            if (deathArea != null)
                deathArea.KillAllPlayers();

            // Trigger Shake
            LIShakeArea? triggerShake = GetComponent<LIShakeArea>();
            if (triggerShake != null)
                triggerShake.SetEnabled(isEnabled ?? !triggerShake.enabled);
        }

        /// <summary>
        /// Coroutine to run timer trigger. Fires onStart on the start and onFinish on completion.
        /// </summary>
        /// <param name="orgin">Player of orgin</param>
        /// <param name="stackSize">Size of the trigger stack</param>
        [HideFromIl2Cpp]
        private IEnumerator CoTimerTrigger(PlayerControl? orgin, int stackSize = 1)
        {
            do
            {
                Trigger(gameObject, "onStart", orgin, stackSize);
                float duration = _sourceElem?.properties.triggerTime ?? 1;
                yield return new WaitForSeconds(duration);
                Trigger(gameObject, "onFinish", orgin, stackSize);
            }
            while (_sourceElem?.properties.triggerLoop ?? false);
            _timerCoroutine = null;
        }

        /// <summary>
        /// Stops the timer coroutine if it is running
        /// </summary>
        public void StopTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
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
