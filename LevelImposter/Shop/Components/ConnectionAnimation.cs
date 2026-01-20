using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace LevelImposter.Shop;

public class ConnectionAnimation(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public Il2CppValueField<Vector3> startPosition;
    public Il2CppValueField<Vector3> endPosition;
    public Il2CppValueField<int> dotCount;
    public Il2CppReferenceField<SpriteRenderer> dotPrefab;
    public Il2CppValueField<float> fadePercentage;
    public Il2CppValueField<float> animationDuration;

    private bool _isReverse = false;
    private readonly List<SpriteRenderer> _dots = [];

    public void Start()
    {
        for (var i = 0; i < dotCount; i++)
            _dots.Add(Instantiate(dotPrefab.Value, transform));
    }
    public void OnEnable()
    {
        StartCoroutine(CoAnimateDots().WrapToIl2Cpp());
    }

    [HideFromIl2Cpp]
    private IEnumerator CoAnimateDots()
    {
        var t = 0.0f;
        while (true)
        {
            var durationPerDot = animationDuration / dotCount;
            t += Time.deltaTime;

            // Update each dot
            for (var i = 0; i < _dots.Count; i++)
            {
                // Calculate progress with offset
                var dotTimeOffset = t + (i * durationPerDot);
                var progress = (dotTimeOffset % animationDuration) / animationDuration;
                if (_isReverse)
                    progress = 1.0f - progress;

                // Position
                _dots[i].transform.localPosition = Vector3.Lerp(startPosition, endPosition, progress);

                // Opacity
                float opacity;
                if (progress - fadePercentage < 0)
                    opacity = progress / fadePercentage;
                else if (progress > 1.0f - fadePercentage)
                    opacity = (1.0f - progress) / fadePercentage;
                else
                    opacity = 1.0f;

                _dots[i].color = new Color(
                    _dots[i].color.r,
                    _dots[i].color.g,
                    _dots[i].color.b,
                    opacity);

            }

            // Wait a frame
            yield return null;
        }
    }
    
    /// <summary>
    /// Sets whether the animation is in reverse.
    /// </summary>
    /// <param name="isReverse">True to reverse the animation, false for normal direction.</param>
    public void SetReverse(bool isReverse)
    {
        _isReverse = isReverse;
    }
}