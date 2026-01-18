using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.AssetLoader;
using LevelImposter.Builders;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Component to animate LI's Sprite Animations in-game
/// </summary>
public class SpriteAnimator(IntPtr intPtr) : LIAnimatorBase(intPtr)
{
    private SpriteBuilder? _spriteBuilder;
    private LISpriteAnimation? _currentAnimation;
    private LISpriteAnimation[]? _allAnimations;
        
    [HideFromIl2Cpp]
    public void Init(LIElement element, LISpriteAnimation[] animations, MapTarget mapTarget)
    {
        _spriteBuilder = new SpriteBuilder(mapTarget);
        _allAnimations = animations;
        SetAnimationType("default");
        Init(element);
    }
    
    public override void PlayType(string type)
    {
        LILogger.Info($"Playing animation {type} on {name}");
        SetAnimationType(type);
        
        var isDefault = type == "default";
        Play(isDefault, false);
    }
    
    protected override int GetFrameCount()
    {
        return _currentAnimation?.frames.Length ?? 0;
    }
    
    protected override Sprite GetFrameSprite(int frameIndex)
    {
        // Get Frame Data
        var frame = GetFrameData(frameIndex);
        
        // Get Loadable Sprite
        var loadableSprite = _spriteBuilder?.GetLoadableFromID(frame.spriteID);
        if (loadableSprite == null)
            throw new Exception("Animation sprite loadable not found");
        
        // Load and return the sprite
        return SpriteLoader.Instance.LoadImmediate((LoadableSprite)loadableSprite).Sprite;
    }

    protected override float GetFrameDelay(int frameIndex)
    {
        var frame = GetFrameData(frameIndex);
        return frame.delay / 1000f;
    }

    protected override void OnClone(LIAnimatorBase originalAnim)
    {
        if (originalAnim is not SpriteAnimator originalSpriteAnim)
            return;
        
        _allAnimations = originalSpriteAnim._allAnimations;
        _currentAnimation = originalSpriteAnim._currentAnimation;
    }

    protected override bool IsReady()
    {
        return _currentAnimation != null;
    }

    /// <summary>
    /// Gets the frame data for the given index
    /// </summary>
    /// <param name="frameIndex">Index of the frame, starting from 0. Wraps around if greater than frame count.</param>
    /// <returns>The frame data</returns>
    /// <exception cref="InvalidOperationException">Thrown if the animation is not initialized</exception>
    [HideFromIl2Cpp]
    private LISpriteAnimationFrame GetFrameData(int frameIndex)
    {
        if (_currentAnimation == null)
            throw new InvalidOperationException("Animation not initialized");
        
        return _currentAnimation.frames[frameIndex % _currentAnimation.frames.Length];
    }
    
    /// <summary>
    /// Sets the current animation based on type
    /// </summary>
    /// <param name="type">Type of animation to set</param>
    private void SetAnimationType(string type)
    {
        _currentAnimation = _allAnimations?.FirstOrDefault(anim => anim.type == type && anim.frames.Length > 0);
    }
}