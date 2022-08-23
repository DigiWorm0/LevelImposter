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
            if (elem.properties.soundIDs == null)
            {
                LILogger.Warn("Step sound missing audio data");
                return;
            }

            SoundGroup soundGroup = ScriptableObject.CreateInstance<SoundGroup>();
            soundGroup.Clips = new AudioClip[elem.properties.soundIDs.Length];
            for (int i = 0; i < elem.properties.soundIDs.Length; i++)
            {
                string resourceID = elem.properties.soundIDs[i];
                string resource = MapUtils.GetResource(resourceID);
                if (resource == null)
                {
                    LILogger.Warn("Step sound missing resource data [" + resourceID + "]");
                    continue;
                }
                AudioClip clip = MapUtils.ConvertToAudio(elem.name, resource);
                soundGroup.Clips[i] = clip;
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