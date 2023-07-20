using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Core
{
    /*
     *      Makes Ambient Sounds play
     *      as "Music" instead of "SFX"
     */
    [HarmonyPatch(typeof(AmbientSoundPlayer), nameof(AmbientSoundPlayer.Start))]
    public static class SoundStartPatch
    {
        public static bool Prefix(AmbientSoundPlayer __instance)
        {
            if (LIShipStatus.Instance == null)
                return true;

            string soundName = __instance.name + __instance.GetInstanceID().ToString();
            SoundManager.Instance.PlayDynamicSound(
                soundName,
                __instance.AmbientSound,
                true,
                new System.Action<AudioSource, float>(__instance.Dynamics), 
                SoundManager.Instance.MusicChannel
            );
            return false;
        }
    }
}