using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace LevelImposter.Shop;

public class FloatingAnimation(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public Il2CppValueField<float> floatSpeed;
    public Il2CppValueField<float> floatAmplitude;
    
    private Vector3 _initialPosition;
        
    public void Start()
    {
        _initialPosition = transform.localPosition;
    }
        
    public void Update()
    {
        var t = (Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f; // Normalized to [0, 1]

        transform.localPosition = _initialPosition + new Vector3(0, Mathf.Lerp(-floatAmplitude, floatAmplitude, t), 0);
    }
}