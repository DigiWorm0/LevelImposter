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

        public string ID = "";
        public LIElement CurrentElem;
        public Guid ElemID = Guid.Empty;
        public Guid? TargetElemID = Guid.Empty;
        public string TargetTriggerID = "";
        public LITriggerable targetTrigger = null;

        public virtual void onTrigger(GameObject orgin)
        {
            LILogger.Info(name + " >>> " + ID + " (" + orgin.name + ")");
            switch (ID)
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
            }
        }

        public void Trigger(GameObject orgin)
        {
            onTrigger(orgin);
            if (targetTrigger != null)
                targetTrigger.Trigger(orgin);
        }

        private IEnumerator CoTimerTrigger(GameObject orgin)
        {
            MapUtils.FireTrigger(gameObject, "onStart", orgin);
            float duration = CurrentElem.properties.triggerTime ?? 1;
            yield return new WaitForSeconds(duration);
            MapUtils.FireTrigger(gameObject, "onFinish", orgin);
        }
    }
}
