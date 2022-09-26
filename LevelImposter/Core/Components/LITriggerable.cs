using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx.IL2CPP.Utils.Collections;

namespace LevelImposter.Core
{
    public class LITriggerable : MonoBehaviour
    {
        public string id = "";
        public LIElement elem;
        public Guid elemID = Guid.Empty;
        public Guid? targetElemID = Guid.Empty;
        public string targetTriggerID = "";

        public LITriggerable targetTrigger = null;
        
        public LITriggerable(IntPtr intPtr) : base(intPtr)
        {
        }

        public virtual void onTrigger(GameObject orgin)
        {
            LILogger.Info(name + " >>> " + id + " (" + orgin.name + ")");
            switch (id)
            {
                case "Enable":
                    gameObject.SetActive(true);
                    break;
                case "Disable":
                    gameObject.SetActive(false);
                    break;
                case "Show":
                    gameObject.SetActive(true);
                    break;
                case "Hide":
                    gameObject.SetActive(false);
                    break;
                case "Repeat":
                    for (int i = 0; i < 8; i++)
                        MapUtils.FireTrigger(gameObject, "onRepeat " + (i + 1), orgin);
                    break;
                case "Start Timer":
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
            float duration = elem.properties.triggerTime ?? 1;
            yield return new WaitForSeconds(duration);
            MapUtils.FireTrigger(gameObject, "onFinish", orgin);
        }
    }
}
