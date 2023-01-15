using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class StepSoundBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-sound2")
                return;

            // Colliders
            Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
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
            SoundGroup soundGroup = ScriptableObject.CreateInstance<SoundGroup>();
            soundGroup.Clips = new AudioClip[elem.properties.sounds.Length];
            for (int i = 0; i < elem.properties.sounds.Length; i++)
            {
                LISound sound = elem.properties.sounds[i];
                if (sound.data == null)
                {
                    LILogger.Warn($"{elem.name} missing audio data");
                    continue;
                }

                if (sound.isPreset) // Preset
                {
                    SoundData soundData;
                    AssetDB.Sounds.TryGetValue(sound.data, out soundData);
                    soundGroup.Clips[i] = soundData.Clip;
                }
                else // WAVLoader
                {
                    WAVLoader.Instance?.LoadWAV(elem, sound, (AudioClip audioClip) =>
                    {
                        soundGroup.Clips[i] = audioClip;
                    });
                }
            }

            // Sound Player
            FootstepWatcher stepPlayer = obj.AddComponent<FootstepWatcher>();
            stepPlayer.Area = colliders[0];
            stepPlayer.Sounds = soundGroup;
            stepPlayer.priority = elem.properties.soundPriority == null ? 0 : (int)elem.properties.soundPriority;
        }

        public void PostBuild() { }
    }
}