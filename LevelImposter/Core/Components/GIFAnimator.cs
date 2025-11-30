using System;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Component to animate GIF data in-game
/// </summary>
public class GIFAnimator(IntPtr intPtr) : LIAnimatorBase(intPtr)
{
    private GIFFile? _gifFile = null;
    
    [HideFromIl2Cpp]
    public void Init(LIElement element, GIFFile gifFile)
    {
        _gifFile = gifFile;
        Init(element);
    }
    
    protected override int GetFrameCount()
    {
        return _gifFile?.Frames.Count ?? 0;
    }
    
    protected override Sprite GetFrameSprite(int frameIndex)
    {
        var sprite = _gifFile?.GetFrameSprite(frameIndex);
        if (sprite == null)
            throw new Exception("GIF frame sprite not found");
        
        return sprite;
    }

    protected override float GetFrameDelay(int frameIndex)
    {
        var frame = GetFrameData(frameIndex);
        return frame.Delay;
    }
    
    protected override void OnClone(LIAnimatorBase originalAnim)
    {
        if (originalAnim is GIFAnimator originalGIFAnim)
            _gifFile = originalGIFAnim._gifFile;
    }
    
    [HideFromIl2Cpp]
    private GIFFile.GIFFrame GetFrameData(int frameIndex)
    {
        if (_gifFile == null)
            throw new Exception("GIF data not initialized");
        
        return _gifFile.Frames[frameIndex % _gifFile.Frames.Count];
    }
}