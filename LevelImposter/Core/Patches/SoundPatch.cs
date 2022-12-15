using HarmonyLib;
using UnityEngine;

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
            if (!__instance.gameObject.active)
            {
                source.volume = 0.0f;
                return false;
            }
            return true;
        }
    }
}