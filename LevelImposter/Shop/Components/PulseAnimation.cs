using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace LevelImposter.Shop.Components;

public class PulseAnimation(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private SpriteRenderer? _spriteRenderer;
    public Il2CppValueField<float> maxOpacity;
    public Il2CppValueField<float> minOpacity;
    public Il2CppValueField<float> pulseSpeed;

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