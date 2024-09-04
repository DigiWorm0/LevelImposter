using System;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that oscillates up and down
/// </summary>
public class LIFloat(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private float _height = 0.2f;
    private float _speed = 2.0f;
    private float _yOffset;
    private float _yScale = 1;

    private float _t => Time.time;

    public void Update()
    {
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            (Mathf.Sin(_t * _speed) + 1) * _yScale * _height / 2 + _yOffset,
            transform.localPosition.z
        );
    }

    [HideFromIl2Cpp]
    public void Init(LIElement elem)
    {
        _height = elem.properties.floatingHeight ?? _height;
        _speed = elem.properties.floatingSpeed ?? _speed;
        _yOffset = elem.y;
        _yScale = elem.yScale;
    }
}