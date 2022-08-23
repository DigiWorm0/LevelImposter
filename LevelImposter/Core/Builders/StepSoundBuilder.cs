using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class StepSoundBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-sound2")
                return;

            // AudioClip
            if (elem.properties.sounds == null)
            {
                LILogger.Warn("Step sound missing audio listing");
                return;
            }

            SoundGroup soundGroup = ScriptableObject.CreateInstance<SoundGroup>();
            soundGroup.Clips = new AudioClip[elem.properties.sounds.Length];
            for (int i = 0; i < elem.properties.sounds.Length; i++)
            {
                LISound sound = elem.properties.sounds[i];

                if (sound.data == null)
                {
                    LILogger.Warn("Step sound missing audio data [" + sound.id + "]");
                    continue;
                }

                AudioClip clip;
                if (sound.isPreset)
                {
                    SoundData soundData;
                    AssetDB.sounds.TryGetValue(sound.data, out soundData);
                    clip = soundData.Clip;
                }
                else
                {
                    clip = MapUtils.ConvertToAudio(elem.name, sound.data);
                }

                if (clip != null)
                    soundGroup.Clips[i] = clip;
                else
                    LILogger.Warn("Step sound has corrupt audio data [" + sound.id + "]");
            }

            // Colliders
            Collider2D[] colliders = obj.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                collider.isTrigger = true;
            }
            if (colliders.Length < 1)
            {
                LILogger.Warn("Step sound missing cooresponding collision");
                return;
            }

            // Sound Player
            FootstepWatcher stepPlayer = obj.AddComponent<FootstepWatcher>();
            stepPlayer.Area = colliders[0];
            stepPlayer.Sounds = soundGroup;
            stepPlayer.priority = elem.properties.soundPriority == null ? 0 : (int)elem.properties.soundPriority;
        }

        public void PostBuild() {}
    }
}