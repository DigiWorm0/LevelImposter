using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace LevelImposter.Shop.Components;

/// <summary>
///     Represents a flexible grid of positioned game objects
/// </summary>
public class GameObjectGrid(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private int _gameObjectCount;

    private Scroller? _scroller;

    // Serialized Fields
    public Il2CppValueField<int> maxColumns = null!;
    public Il2CppValueField<float> xSpacing = null!;
    public Il2CppValueField<float> ySpacing = null!;

    public void Awake()
    {
        _scroller = GetComponentInParent<Scroller>();
    }

    /// <summary>
    ///     Resets all child GameObjects in the grid
    /// </summary>
    public void Reset()
    {
        _gameObjectCount = 0;

        // Reset Scroll Height
        if (_scroller != null)
            _scroller.ContentYBounds.max = 0;
    }

    /// <summary>
    ///     Adds a Transform to the grid
    /// </summary>
    /// <param name="childTransform">The Transform to add</param>
    public void AddTransform(Transform childTransform)
    {
        // Calculate Row and Column
        var row = _gameObjectCount / maxColumns.Value;
        var column = _gameObjectCount % maxColumns.Value;
        _gameObjectCount++;

        // Position GameObject
        childTransform.transform.SetParent(transform);
        childTransform.transform.localPosition = new Vector3(
            column * xSpacing.Value - (maxColumns.Value - 1) * xSpacing.Value * 0.5f,
            -row * ySpacing.Value,
            0
        );

        // Set Scroll Height
        if (_scroller != null)
            _scroller.ContentYBounds.max = row * ySpacing.Value + ySpacing.Value;
    }
}