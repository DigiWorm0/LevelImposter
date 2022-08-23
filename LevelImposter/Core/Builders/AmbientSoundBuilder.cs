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
            if (elem.properties.soundID == null)
            {
                LILogger.Warn("Ambient sound missing audio data");
                return;
            }
            string resource = MapUtils.GetResource(elem.properties.soundID);
            if (resource == null)
            {
                LILogger.Warn("Ambient sound missing resource data [" + elem.properties.soundID + "]");
                return;
            }
            AudioClip clip = MapUtils.ConvertToAudio(elem.name, resource);

            // Sound Player
            AmbientSoundPlayer ambientPlayer = obj.AddComponent<AmbientSoundPlayer>();
            ambientPlayer.HitAreas = colliders;
            ambientPlayer.AmbientSound = clip;
            ambientPlayer.MaxVolume = elem.properties.soundVolume == null ? 1 : (float)elem.properties.soundVolume;
        }

        public void PostBuild() {}
    }
}