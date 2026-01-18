using System;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelImposter.Core;

/// <summary>
///     Object that flies across screen and respawns when it hits the edge
/// </summary>
public class LIStar(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private Vector3 _initialScale;

    private float _currentSize;
    private float _currentSpeed;
    
    private float _height = 10;
    private float _length = 10;
    private float _maxSpeed = 2;
    private float _minSpeed = 2;
    
    private float _minSize = 0.2f;
    private float _maxSize = 1.0f;
    private bool _scaleSpeedBySize = true;
    
    public void Start()
    {
        _initialScale = transform.localScale;
        Respawn(true);
    }

    public void Update()
    {
        var sizeScalingFactor = _scaleSpeedBySize ? _currentSize : 1.0f;
        transform.localPosition -= new Vector3(
            _currentSpeed * Time.deltaTime * sizeScalingFactor,
            0,
            0
        );
        if (transform.localPosition.x < -_length)
            Respawn(false);
    }

    /// <summary>
    ///     Initializes a star from util-starfield
    /// </summary>
    /// <param name="elem">LIElement to extract props from</param>
    [HideFromIl2Cpp]
    public void Init(LIElement elem)
    {
        _height = elem.properties.starfieldHeight ?? _height;
        _length = elem.properties.starfieldLength ?? _length;
        _minSpeed = elem.properties.starfieldMinSpeed ?? _minSpeed;
        _maxSpeed = elem.properties.starfieldMaxSpeed ?? _maxSpeed;
        // TODO: Import properties
    }

    /// <summary>
    ///     Respawns the Star in the Star Field
    /// </summary>
    /// <param name="isInitial">TRUE will also randomize the X position</param>
    private void Respawn(bool isInitial)
    {
        _currentSpeed = Random.Range(_minSpeed, _maxSpeed);
        _currentSize = Random.Range(_minSize, _maxSize);
        
        transform.localPosition = new Vector3(
            isInitial ? Random.Range(-_length, 0) : 0,
            Random.Range(-_height / 2, _height / 2),
            0
        );
        transform.localScale = _initialScale * _currentSize;
    }
}