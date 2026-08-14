using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.AssetLoader.FileContainers;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Core.Components;

/// <summary>
///     Component to animate GIF data in-game
/// </summary>
public class GIFAnimator(IntPtr intPtr) : LIAnimatorBase(intPtr)
{
    private static readonly List<string> AUTOPLAY_BLACKLIST =
    [
        "util-vent1",
        "util-vent2",
        "sab-doorv",
        "sab-doorh",
        "util-cam"
    ];

    private GIFFile? _gifFile;
    private string _id = string.Empty;

    [HideFromIl2Cpp]
    public void Init(LIElement element, GIFFile gifFile)
    {
        if (gifFile == null || !gifFile.IsLoaded)
            throw new Exception("GIF data is not fully loaded");

        _id = element.id.ToString();
        _gifFile = gifFile;

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
        return _gifFile?.GetFrameSprite(frameIndex) ?? throw new Exception("GIF data not initialized");
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

    protected override bool IsReady()
    {
        return _gifFile?.IsLoaded ?? false;
    }

    [HideFromIl2Cpp]
    private GIFFile.GIFFrame GetFrameData(int frameIndex)
    {
        if (_gifFile == null)
            throw new Exception("GIF data not initialized");

        return _gifFile.Frames[frameIndex % _gifFile.Frames.Count];
    }
}