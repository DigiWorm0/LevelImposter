using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.DB;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

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

        private GameObject? _triggerTarget = null;
        private string _triggerID = "";

        /// <summary>
        /// Sets spawnable trigger properties
        /// </summary>
        /// <param name="triggerTarget">GameObject to trigger</param>
        /// <param name="triggerID">ID of the trigger</param>
        public void SetTrigger(GameObject triggerTarget, string triggerID)
        {
            _triggerTarget = triggerTarget;
            _triggerID = triggerID;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Coroutine that fires the trigger once the LocalPlayer is spawned in
        /// </summary>
        [HideFromIl2Cpp]
        private IEnumerator CoFireTrigger()
        {
            while (PlayerControl.LocalPlayer == null)
                yield return null;
            if (_triggerTarget != null)
                LITriggerable.Trigger(_triggerTarget, _triggerID, PlayerControl.LocalPlayer);
        }

        public void Start()
        {
            if (_triggerTarget == null || _triggerID == "")
                LILogger.Warn("A Spawnable Trigger enabled without a target");
            StartCoroutine(CoFireTrigger().WrapToIl2Cpp());
        }
    }
}