using System;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using UnityEngine.Audio;

namespace LevelImposter.Core;

/// <summary>
///     Allows controlled audio playback from a trigger input
/// </summary>
public class TriggerSoundPlayer(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private AudioMixerGroup? _channel;
    private AudioClip? _clip;
    private Collider2D[]? _colliders;

    private float _volume = 1.0f;

    public string SoundName => name + GetInstanceID();
    public bool IsPlaying => _clip ? SoundManager.Instance.SoundIsPlaying(_clip) : false;

    public void OnDestroy()
    {
        Stop();

        _clip = null;
        _colliders = null;
        _channel = null;
    }

    /// <summary>
    ///     Initializes the trigger sound
    /// </summary>
    /// <param name="soundData">Sound to play on enter/exit</param>
    /// <param name="colliders">An array of all colliders to trigger from</param>
    [HideFromIl2Cpp]
    public void Init(LISound soundData, Collider2D[] colliders)
    {
        _clip = WAVFile.LoadSound(soundData);
        _volume = soundData?.volume ?? 1.0f;
        _colliders = colliders;
        _channel = soundData?.channel switch
        {
            "sfx" => SoundManager.Instance.SfxChannel,
            "music" => SoundManager.Instance.MusicChannel,
            "ambient" => SoundManager.Instance.AmbienceChannel,
            _ => SoundManager.Instance.SfxChannel
        };
    }

    /// <summary>
    ///     Plays the trigger sound to players within the collider
    /// </summary>
    /// <param name="isLoop"><c>true</c> if the audio should loop continuously, <c>false</c> otherwise</param>
    public void Play(bool isLoop)
    {
        SoundManager.Instance.PlayDynamicSound(
            SoundName,
            _clip,
            isLoop,
            new Action<AudioSource, float>(SoundDynamics),
            _channel
        );
    }

    /// <summary>
    ///     Stops the current trigger sound
    /// </summary>
    public void Stop()
    {
        SoundManager.Instance.StopNamedSound(SoundName);
    }

    /// <summary>
    ///     Delegate to control the volume of the audio source based on player position and state
    /// </summary>
    /// <param name="source">AudioSource to play from</param>
    /// <param name="dt">Change in time</param>
    private void SoundDynamics(AudioSource source, float dt)
    {
        var isEnabled = enabled && gameObject.activeInHierarchy;
        var inMeeting = GameState.IsInMeeting;
        var isPlayer = PlayerControl.LocalPlayer != null;

        // Check player and state
        if (!isEnabled || inMeeting || !isPlayer)
        {
            source.volume = 0f;
            return;
        }

        // Set Volume
        var truePosition = PlayerControl.LocalPlayer?.GetTruePosition() ?? Vector2.zero;
        var isInCollider = _colliders?.Any(c => c.OverlapPoint(truePosition)) ?? false;
        var targetVolume = isInCollider ? _volume : 0;
        source.volume = Mathf.Lerp(source.volume, targetVolume, dt);
    }
}