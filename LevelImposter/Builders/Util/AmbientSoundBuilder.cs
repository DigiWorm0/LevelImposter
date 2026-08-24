using System;
using LevelImposter.AssetLoader;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class AmbientSoundBuilder
{
    private const string AMBIENT_SOUND_TYPE = "util-sound1";
    private const string TRIGGER_SOUND_TYPE = "util-triggersound";

    [ElementBuilder(ElementTypes = [AMBIENT_SOUND_TYPE, TRIGGER_SOUND_TYPE])]
    public static void Build(LIMap map, LIElement element, GameObject gameObject)
    {
        var isAmbient = element.type == AMBIENT_SOUND_TYPE;
        var isTrigger = element.type == TRIGGER_SOUND_TYPE;

        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // AudioClip
        if (element.properties.sounds == null)
        {
            LILogger.Warn($"{element.name} missing audio listing");
            return;
        }

        if (element.properties.sounds.Length <= 0)
        {
            LILogger.Warn($"{element.name} missing audio elements");
            return;
        }

        var soundData = element.properties.sounds[0];
        if (soundData.dataID == null)
        {
            LILogger.Warn($"{element.name} missing audio data ID");
            return;
        }

        // Sound Player
        var isLobby = map.MapTarget == MapTarget.Lobby;
        if (isAmbient)
        {
            var ambientPlayer = gameObject.AddComponent<AmbientSoundPlayer>();
            ambientPlayer.HitAreas = colliders;
            ambientPlayer.MaxVolume = soundData?.volume ?? 1f;

            // Load synchronously
            // Note: Don't load async, this ensures that AmbientSound is defined on Start()
            ambientPlayer.AmbientSound = AudioLoader.LoadSync(
                soundData?.dataID ?? Guid.Empty,
                isLobby);
        }
        else if (isTrigger)
        {
            var triggerPlayer = gameObject.AddComponent<TriggerSoundPlayer>();
            triggerPlayer.Init(soundData, colliders, isLobby);
        }
    }
}