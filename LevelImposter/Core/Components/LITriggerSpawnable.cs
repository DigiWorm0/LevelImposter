using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Trigger;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Fires a trigger on creation. Used for spawnable prefabs.
/// </summary>
public class LITriggerSpawnable(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private string _triggerID = "";

    private GameObject? _triggerTarget;

    public void Start()
    {
        if (_triggerTarget == null || _triggerID == "")
            LILogger.Warn("A Spawnable Trigger enabled without a target");
        StartCoroutine(CoFireTrigger().WrapToIl2Cpp());
    }

    public void OnDestroy()
    {
        _triggerID = "";
        _triggerTarget = null;
    }

    /// <summary>
    ///     Sets spawnable trigger properties
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
    ///     Coroutine that fires the trigger once the LocalPlayer is spawned in
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoFireTrigger()
    {
        while (PlayerControl.LocalPlayer == null
               || LIShipStatus.GetInstanceOrNull()?.IsReady != true
               || (!GameManager.Instance.GameHasStarted && GameManager.Instance.ShouldCheckForGameEnd)
               || !LagLimiter.ShouldContinue(30))
            yield return null;

        if (_triggerTarget != null)
        {
            TriggerSignal signal = new(_triggerTarget, _triggerID, PlayerControl.LocalPlayer);
            TriggerSystem.GetInstance().FireTrigger(signal);
        }
    }
}