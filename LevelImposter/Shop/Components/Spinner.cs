using System;
using UnityEngine;

namespace LevelImposter.Shop.Components;

/// <summary>
///     Just a simple spinning object
/// </summary>
public class Spinner(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private const float SPINNER_SPEED = -90f;

    public void Update()
    {
        transform.Rotate(0, 0, SPINNER_SPEED * Time.deltaTime);
    }
}