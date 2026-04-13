using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace LevelImposter.Shop;

public class PulseAnimation(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public Il2CppValueField<float> pulseSpeed;
    public Il2CppValueField<float> minOpacity;
    public Il2CppValueField<float> maxOpacity;

    private SpriteRenderer? _spriteRenderer;
    
    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Update()
    {
        var t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // Normalized to [0, 1]
        
        _spriteRenderer?.color = new Color(
            _spriteRenderer.color.r,
            _spriteRenderer.color.g,
            _spriteRenderer.color.b,
            Mathf.Lerp(minOpacity, maxOpacity, t)
        );
    }
}