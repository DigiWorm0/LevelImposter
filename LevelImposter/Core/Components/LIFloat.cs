using System;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that oscillates up and down
/// </summary>
public class LIFloat(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private float _height = 0.2f;
    private Vector3 _lastPosition;
    private float _speed = 2.0f;
    private float _yScale = 1;

    public void Awake()
    {
        // Get LI data
        var objectData = gameObject.GetLIData();
        if (objectData == null)
            throw new Exception("LIFloat is missing LI data");

        _height = objectData.Element.properties.floatingHeight ?? _height;
        _speed = objectData.Element.properties.floatingSpeed ?? _speed;
        _yScale = objectData.Element.yScale;
    }

    public void Update()
    {
        // Oscillate
        var time = Time.time;
        var value = (Mathf.Sin(time * _speed) + 1) * _yScale * _height / 2;
        var rotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;

        // Get new position
        var newPosition = new Vector3(
            Mathf.Sin(rotation) * value,
            Mathf.Cos(rotation) * value,
            0
        );

        // Move object by delta
        var delta = newPosition - _lastPosition;
        transform.position += delta;

        // Update last position
        _lastPosition = newPosition;
    }
}