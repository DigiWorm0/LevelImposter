using LevelImposter.DB;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Builders
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
                // Sound Data
                LISound sound = elem.properties.sounds[i];
                if (sound.data == null)
                {
                    LILogger.Warn($"{elem.name} missing audio data");
                    continue;
                }

                // Preset
                if (sound.isPreset)
                {
                    soundGroup.Clips[i] = AssetDB.GetSound(sound.data);
                }
                // WAVLoader
                else
                {
                    soundGroup.Clips[i] = WAVFile.Load(sound.data);
                }
            }

            // Sound Player
            FootstepWatcher stepPlayer = obj.AddComponent<FootstepWatcher>();
            stepPlayer.Area = colliders[0];
            stepPlayer.Sounds = soundGroup;
            stepPlayer.priority = elem.properties.soundPriority ?? 0;
        }

        public void PostBuild() { }
    }
}