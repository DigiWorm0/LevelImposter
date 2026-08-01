using UnityEngine;

namespace LevelImposter.AssetLoader;

public class AudioResult(AudioClip audioClip) : ICachable
{
    public AudioClip AudioClip => audioClip;
    public bool IsExpired => audioClip == null;

    public static implicit operator AudioClip(AudioResult audioResult)
    {
        return audioResult.AudioClip;
    }
}