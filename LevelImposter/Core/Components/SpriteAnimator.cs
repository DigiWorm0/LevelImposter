using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.AssetLoader;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Component to animate LI's Sprite Animations in-game
/// </summary>
public class SpriteAnimator(IntPtr intPtr) : LIAnimatorBase(intPtr)
{
    private LISpriteAnimation? _animation;
    
    [HideFromIl2Cpp]
    public void Init(LIElement element, LISpriteAnimation animation)
    {
        _animation = animation;
        Init(element);
    }
    
    protected override int GetFrameCount()
    {
        return _animation?.frames.Length ?? 0;
    }
    
    protected override Sprite GetFrameSprite(int frameIndex)
    {
        // Get Frame Data
        var frame = GetFrameData(frameIndex);
        
        // Get MapAsset
        var mapAssetDB = MapLoader.CurrentMap?.mapAssetDB;
        var mapAsset = mapAssetDB?.Get(frame.spriteID);
        if (mapAsset == null)
            throw new Exception("Animation sprite not found in MapAssetDB");

        // Load Sprite
        // TODO: Do this asynchronously
        return SpriteLoader.LoadSync(
            frame.spriteID.ToString(),
            mapAsset
        );
    }

    protected override float GetFrameDelay(int frameIndex)
    {
        var frame = GetFrameData(frameIndex);
        return frame.delay / 1000f;
    }

    protected override void OnClone(LIAnimatorBase originalAnim)
    {
        if (originalAnim is SpriteAnimator originalSpriteAnim)
            _animation = originalSpriteAnim._animation;
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
        if (_animation == null)
            throw new InvalidOperationException("Animation not initialized");
        
        return _animation.frames[frameIndex % _animation.frames.Length];
    }
}