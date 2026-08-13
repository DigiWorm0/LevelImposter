using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace LevelImposter.Shop.Components;

public class FloatingAnimation(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private Vector3 _initialPosition;
    public Il2CppValueField<float> floatAmplitude = null!;
    public Il2CppValueField<float> floatSpeed = null!;

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