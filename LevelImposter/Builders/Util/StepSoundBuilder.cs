using LevelImposter.AssetLoader;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

internal class StepSoundBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-sound2")
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;
        if (colliders.Length < 1)
        {
            LILogger.Warn($"{elem.name} missing cooresponding collision");
            return;
        }

        // AudioClip
        if (elem.properties.sounds == null)
        {
            LILogger.Warn($"{elem.name} missing audio listing");
            return;
        }

        // Sound Group
        var soundGroup = ScriptableObject.CreateInstance<SoundGroup>();
        soundGroup.Clips = new AudioClip[elem.properties.sounds.Length];
        for (var i = 0; i < elem.properties.sounds.Length; i++)
        {
            // Sound Data
            var sound = elem.properties.sounds[i];
            if (sound == null)
            {
                LILogger.Warn($"{elem.name} missing audio data");
                continue;
            }

            // Preset
            if (sound.isPreset)
                soundGroup.Clips[i] = AssetDB.GetSound(sound.presetID ?? "");
            // WAVLoader
            else
                soundGroup.Clips[i] = WAVLoader.Load(sound);
        }

        // Sound Player
        var stepPlayer = obj.AddComponent<FootstepWatcher>();
        stepPlayer.Area = colliders[0];
        stepPlayer.Sounds = soundGroup;
        stepPlayer.priority = elem.properties.soundPriority ?? 0;
    }
}