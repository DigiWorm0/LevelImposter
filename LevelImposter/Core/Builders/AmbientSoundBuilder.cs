using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class AmbientSoundBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-sound1")
                return;

            // Colliders
            Collider2D[] colliders = obj.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                collider.isTrigger = true;
            }

            // AudioClip
            if (elem.properties.sounds == null)
            {
                LILogger.Warn("Ambient sound missing audio listing");
                return;
            }

            if (elem.properties.sounds.Length <= 0)
            {
                LILogger.Warn("Ambient sound missing audio elements");
                return;
            }

            LISound soundData = elem.properties.sounds[0];
            if (soundData.data == null)
            {
                LILogger.Warn("Ambient sound missing audio data [" + soundData.id + "]");
                return;
            }
            AudioClip clip = MapUtils.ConvertToAudio(elem.name, soundData.data);

            // Sound Player
            AmbientSoundPlayer ambientPlayer = obj.AddComponent<AmbientSoundPlayer>();
            ambientPlayer.HitAreas = colliders;
            ambientPlayer.AmbientSound = clip;
            ambientPlayer.MaxVolume = soundData.volume;
        }

        public void PostBuild() {}
    }
}