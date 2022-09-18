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

        public virtual void onTrigger()
        {
            LILogger.Info(name + " >>> " + id);
            switch (id)
            {
                case "Enable":
                    gameObject.SetActive(true);
                    break;
                case "Disable":
                    gameObject.SetActive(false);
                    break;
            }
        }

        public void Trigger()
        {
            onTrigger();
            if (targetTrigger != null)
                targetTrigger.Trigger();
        }
    }
}
