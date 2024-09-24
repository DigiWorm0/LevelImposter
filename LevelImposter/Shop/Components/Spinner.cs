using System;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
///     Just a simple spinning object
/// </summary>
public class Spinner(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private readonly float _speed = -90f;

    public void Update()
    {
        transform.Rotate(0, 0, _speed * Time.deltaTime);
    }
}