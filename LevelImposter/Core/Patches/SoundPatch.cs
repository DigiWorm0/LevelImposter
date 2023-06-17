using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Core
{
    /*
     *      Makes isActive & inMeeting a dependency
     *      for Ambient Sounds to play
     */
    [HarmonyPatch(typeof(AmbientSoundPlayer), nameof(AmbientSoundPlayer.Dynamics))]
    public static class SoundDynamicsPatch
    {
        public static bool Prefix([HarmonyArgument(0)] AudioSource source, AmbientSoundPlayer __instance)
        {
            if (LIShipStatus.Instance == null)
                return true;

            bool isActive = __instance.gameObject.active;
            bool inMeeting = MeetingHud.Instance != null;
            if (!isActive || inMeeting)
            {
                source.volume = 0.0f;
                return false;
            }
            return true;
        }
    }

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