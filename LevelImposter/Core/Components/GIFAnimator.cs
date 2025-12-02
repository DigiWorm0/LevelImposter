using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Component to animate GIF data in-game
/// </summary>
public class GIFAnimator(IntPtr intPtr) : LIAnimatorBase(intPtr)
{
    private string _id = string.Empty;
    private GIFFile? _gifFile;
    private Sprite?[] _frameSprites = Array.Empty<Sprite>();

    private static readonly List<string> AUTOPLAY_BLACKLIST =
    [
        "util-vent1",
        "util-vent2",
        "sab-doorv",
        "sab-doorh",
        "util-cam"
    ];
    
    [HideFromIl2Cpp]
    public void Init(LIElement element, GIFFile gifFile)
    {
        if (!_gifFile?.IsLoaded ?? true)
            throw new Exception("GIF data is not fully loaded");

        _id = element.id.ToString();
        _gifFile = gifFile;
        _frameSprites = new Sprite[_gifFile.Frames.Count];
        
        // Initialize base
        Init(element);
        
        // Stop autoplay for certain elements
        var door = GetComponent<PlainDoor>();
        if (AUTOPLAY_BLACKLIST.Contains(element.type)) // Don't autoplay
            Stop(door && !door.IsOpen); // Jump to end if door is closed
    }

    public override void PlayType(string type)
    {
        switch (type)
        {
            case "openDoor":
            case "exitVent":
                Play(false, true);
                break;
            case "closeDoor":
            case "enterVent":
                Play(false, false);
                break;
            case "camsInactive":
                Stop();
                break;
            default:
                Play();
                break;
        }
    }
    
    protected override int GetFrameCount()
    {
        return _gifFile?.Frames.Count ?? 0;
    }
    
    protected override Sprite GetFrameSprite(int frameIndex)
    {
        // Check to see if we have the sprite cached
        if (_frameSprites.Length > frameIndex && _frameSprites[frameIndex] != null)
            return _frameSprites[frameIndex];
        
        // Load the texture for the frame
        var texture = _gifFile?.GetFrameTexture(frameIndex);
        if (texture == null)
            throw new Exception("GIF frame sprite not found");
        
        // Create the sprite
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100.0f,
            0,
            SpriteMeshType.FullRect
        );
        
        // Set Sprite Flags
        sprite.name = $"{_id}_gif_{frameIndex}";
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        
        // Register in GC
        GCHandler.Register(sprite);
        
        // Cache the sprite
        _frameSprites[frameIndex] = sprite;
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