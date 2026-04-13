using System;
using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using LevelImposter.Lobby;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

public class ProgressBar(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    // Serialized fields
    public Il2CppReferenceField<SpriteRenderer> progressBarBackground;
    public Il2CppReferenceField<SpriteRenderer> progressBarFill;

    private const float ANIMATION_SPEED = 10.0f;
    private float _progress;
    private Color? _color;

    public void Update()
    {
        // Update progress animation
        var currentProgress = progressBarFill.Value.size.x / progressBarBackground.Value.size.x;
        if (!Mathf.Approximately(_progress, currentProgress))
        {
            // Animate the fill towards the target
            var deltaProgress = (currentProgress - _progress) * ANIMATION_SPEED * Time.deltaTime;
            var newProgress = currentProgress - deltaProgress;
            SetActualProgress(newProgress);

        }
        
        // Update color if set
        var currentColor = progressBarFill.Value.color;
        var targetColor = _color ?? progressBarFill.Value.color;
        if (currentColor == targetColor)
            return;
        
        // Continue to animate color
        var color = Color.Lerp(currentColor, targetColor, ANIMATION_SPEED * Time.deltaTime);
        progressBarFill.Value.color = color;
    }
    
    /// <summary>
    /// Sets the actual size of the progress bar fill without animation.
    /// </summary>
    /// <param name="progress">Progress value between 0 and 1.</param>
    private void SetActualProgress(float progress)
    {
        var fillSize = progress * progressBarBackground.Value.size.x;
        progressBarFill.Value.size = new Vector2(fillSize, progressBarFill.Value.size.y);
    }
    
    /// <summary>
    /// Sets the progress of the progress bar (0 to 1).
    /// </summary>
    /// <param name="progress">Progress value between 0 and 1.</param>
    public void SetProgress(float progress)
    {
        _progress = Mathf.Clamp01(progress);
        if (!gameObject.activeInHierarchy)
            SetActualProgress(progress);
    }
    
    /// <summary>
    /// Sets the color of the progress bar fill.
    /// </summary>
    /// <param name="color">Color to set the fill to.</param>
    public void SetColor(Color color)
    {
        _color = color;
    }
}