using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

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

        private static List<LITriggerable> _allTriggers = new List<LITriggerable>();
        public static List<LITriggerable> AllTriggers => _allTriggers;

        private LIElement _sourceElem;
        private Guid? _sourceID => _sourceElem.id;
        private string _sourceTrigger;
        private Guid? _destID;
        private string _destTrigger;
        private LITriggerable _destTriggerComp;

        public LIElement SourceElem => _sourceElem;
        public Guid? SourceID => _sourceID;
        public string SourceTrigger => _sourceTrigger;
        public Guid? DestID => _destID;
        public string DestTrigger => _destTrigger;
        public LITriggerable DestTriggerComp => _destTriggerComp;

        public void Awake()
        {
            _allTriggers.Add(this);
        }

        public void Start()
        {
            _destTriggerComp = AllTriggers.Find(t => _destID == t._sourceID && _destTrigger == t._sourceTrigger);
        }

        public void OnDestroy()
        {
            _allTriggers.Remove(this);
        }

        public void SetTrigger(LIElement sourceElem, string sourceTrigger, Guid? destID, string destTrigger)
        {
            _sourceElem = sourceElem;
            _sourceTrigger = sourceTrigger;
            _destID = destID;
            _destTrigger = destTrigger;
        }

        public virtual void OnTrigger(PlayerControl orgin)
        {
            LILogger.Info(name + " >>> " + _sourceTrigger + " (" + orgin.name + ")");
            switch (_sourceTrigger)
            {
                case "enable":
                    gameObject.SetActive(true);
                    break;
                case "disable":
                    gameObject.SetActive(false);
                    break;
                case "show":
                    gameObject.SetActive(true);
                    break;
                case "hide":
                    gameObject.SetActive(false);
                    break;
                case "repeat":
                    for (int i = 0; i < 8; i++)
                        MapUtils.FireTrigger(gameObject, "onRepeat " + (i + 1), orgin);
                    break;
                case "startTimer":
                    StartCoroutine(CoTimerTrigger(orgin).WrapToIl2Cpp());
                    break;
                case "open":
                    SetDoorOpen(true);
                    break;
                case "close":
                    SetDoorOpen(false);
                    break;

            }
        }

        public void Trigger(PlayerControl orgin)
        {
            OnTrigger(orgin);
            if (_destTriggerComp != null)
                _destTriggerComp.Trigger(orgin);
        }

        private IEnumerator CoTimerTrigger(PlayerControl orgin)
        {
            MapUtils.FireTrigger(gameObject, "onStart", orgin);
            float duration = _sourceElem.properties.triggerTime ?? 1;
            yield return new WaitForSeconds(duration);
            MapUtils.FireTrigger(gameObject, "onFinish", orgin);
        }

        private void SetDoorOpen(bool isOpen)
        {
            PlainDoor doorComponent = gameObject.GetComponent<PlainDoor>();
            doorComponent.SetDoorway(isOpen);
        }
    }
}
