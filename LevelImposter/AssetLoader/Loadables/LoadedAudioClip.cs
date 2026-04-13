using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class LoadedAudioClip(AudioClip audioClip) : ICachable
{
    public AudioClip AudioClip => audioClip;
    public bool IsExpired => audioClip == null;
    
    public static implicit operator AudioClip(LoadedAudioClip loadedAudioClip) => loadedAudioClip.AudioClip;
}