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

    public void Update()
    {
        var currentFill = progressBarFill.Value.size.x;
        var targetFill = _progress * progressBarBackground.Value.size.x;
        if (Mathf.Approximately(currentFill, targetFill))
            return;
        
        // Animate the fill towards the target
        var deltaFill = (currentFill - targetFill) * ANIMATION_SPEED * Time.deltaTime;
        var newFill = currentFill - deltaFill;
        progressBarFill.Value.size = new Vector2(newFill, progressBarFill.Value.size.y);
    }
    
    /// <summary>
    /// Sets the progress of the progress bar (0 to 1).
    /// </summary>
    /// <param name="progress">Progress value between 0 and 1.</param>
    public void SetProgress(float progress)
    {
        _progress = Mathf.Clamp01(progress);
    }
}