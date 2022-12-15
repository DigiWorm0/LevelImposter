using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.DB;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace LevelImposter.Core
{
    /// <summary>
    /// Fires a trigger on creation. Used for spawnable prefabs.
    /// </summary>
    public class LITriggerSpawnable : MonoBehaviour
    {
        public LITriggerSpawnable(IntPtr intPtr) : base(intPtr)
        {
        }

        private GameObject _triggerTarget = null;
        private string _triggerID = "";

        public void SetTrigger(GameObject triggerTarget, string triggerID)
        {
            _triggerTarget = triggerTarget;
            _triggerID = triggerID;
            gameObject.SetActive(false);
        }

        public void Start()
        {
            if (_triggerTarget == null || _triggerID == "")
            {
                LILogger.Error("A Spawnable Trigger enabled without a target");
                return;
            }

            MapUtils.FireTrigger(_triggerTarget, _triggerID, gameObject);
        }
    }
}