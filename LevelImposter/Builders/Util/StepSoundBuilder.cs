using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class StepSoundBuilder
{
    [ElementBuilder(ElementTypes = ["util-sound2"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;
        if (colliders.Length < 1)
        {
            LILogger.Warn($"{element.name} missing cooresponding collision");
            return;
        }

        // AudioClip
        if (element.properties.sounds == null)
        {
            LILogger.Warn($"{element.name} missing audio listing");
            return;
        }

        // Sound Group
        var soundGroup = ScriptableObject.CreateInstance<SoundGroup>();
        soundGroup.Clips = new AudioClip[element.properties.sounds.Length];
        for (var i = 0; i < element.properties.sounds.Length; i++)
        {
            // Sound Data
            var sound = element.properties.sounds[i];
            if (sound == null)
            {
                LILogger.Warn($"{element.name} missing audio data");
                continue;
            }

            // Preset
            if (sound.isPreset)
                soundGroup.Clips[i] = PrefabDB.GetSound(sound.presetID ?? "");
            // WAVLoader
            else
                soundGroup.Clips[i] = WAVLoader.Load(sound);
        }

        // Sound Player
        var stepPlayer = gameObject.AddComponent<FootstepWatcher>();
        stepPlayer.Area = colliders[0];
        stepPlayer.Sounds = soundGroup;
        stepPlayer.priority = element.properties.soundPriority ?? 0;
    }
}