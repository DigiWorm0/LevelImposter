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
    private Guid _currentAnimationID = Guid.Empty;
    private LISpriteAnimation? _defaultAnimation;
    private LISpriteAnimation[]? _animations;
    
    [HideFromIl2Cpp]
    public void Init(LIElement element, LISpriteAnimation[] animations)
    {
        _animations = animations;
        _defaultAnimation = animations.FirstOrDefault(anim => anim.type == "default");
        _currentAnimationID = _defaultAnimation?.id ?? Guid.Empty;
        Init(element);
    }
    
    [HideFromIl2Cpp]
    private LISpriteAnimation? GetCurrentAnimation()
    {
        if (_animations == null)
            throw new InvalidOperationException("Animations not initialized");
        
        // Avoid using LINQ for IL2CPP compatibility
        foreach (var animation in _animations)
        {
            if (animation.id == _currentAnimationID)
                return animation;
        }
        return null;
    }
    
    public override void PlayType(string type)
    {
        SetAnimationType(type);
        
        var isDefault = type == "default";
        Play(isDefault, false);
    }
    
    protected override int GetFrameCount()
    {
        return GetCurrentAnimation()?.frames.Length ?? 0;
    }
    
    protected override Sprite GetFrameSprite(int frameIndex)
    {
        // Get Frame Data
        var frame = GetFrameData(frameIndex);
        
        // Get Loadable Sprite
        var loadableSprite = SpriteBuilder.GetLoadableFromID(frame.spriteID);
        if (loadableSprite == null)
            throw new Exception("Animation sprite loadable not found");
        
        // Load and return the sprite
        return SpriteLoader.LoadSync((LoadableSprite)loadableSprite).Sprite;
    }

    protected override float GetFrameDelay(int frameIndex)
    {
        var frame = GetFrameData(frameIndex);
        return frame.delay / 1000f;
    }

    protected override void OnClone(LIAnimatorBase originalAnim)
    {
        if (originalAnim is SpriteAnimator originalSpriteAnim)
        {
            _animations = originalSpriteAnim._animations;
            _defaultAnimation = originalSpriteAnim._defaultAnimation;
            _currentAnimationID = originalSpriteAnim._currentAnimationID;
        }
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
        var animation = GetCurrentAnimation();
        if (animation == null)
            throw new InvalidOperationException("Animation not initialized");
        
        return animation.frames[frameIndex % animation.frames.Length];
    }
    
    /// <summary>
    /// Sets the current animation based on type
    /// </summary>
    /// <param name="type">Type of animation to set</param>
    private void SetAnimationType(string type)
    {
        var animation = _animations?.FirstOrDefault(anim => anim.type == type);
        _currentAnimationID = animation?.id ?? _defaultAnimation?.id ?? Guid.Empty;
    }
}