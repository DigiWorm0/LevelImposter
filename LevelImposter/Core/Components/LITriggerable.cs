using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class LITriggerable : MonoBehaviour
    {
        public string id = "";
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
            }
        }

        public void Trigger(GameObject orgin)
        {
            onTrigger(orgin);
            if (targetTrigger != null)
                targetTrigger.Trigger(orgin);
        }
    }
}
