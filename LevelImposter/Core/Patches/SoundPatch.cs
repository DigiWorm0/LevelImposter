using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Makes isActive a dependency
     *      for Ambient Sounds to play
     */
    [HarmonyPatch(typeof(AmbientSoundPlayer), nameof(AmbientSoundPlayer.Dynamics))]
    public static class SoundEnablePatch
    {
        public static bool Prefix([HarmonyArgument(0)] AudioSource source, AmbientSoundPlayer __instance)
        {
            if (MapLoader.CurrentMap == null)
                return true;

            bool isActive = __instance.gameObject.active;
            bool isMeeting = MeetingHud.Instance != null;
            if (!isActive || isMeeting)
            {
                source.volume = 0.0f;
                return false;
            }
            return true;
        }
    }
}