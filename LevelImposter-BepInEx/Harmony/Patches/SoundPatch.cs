using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.Update))]
    public static class SoundPatch
    {
        public static bool Prefix()
        {
            // TODO Add Sounds / Fix Sound Bug
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.PlayStepSound))]
    public static class StepPatch
    {
        public static bool Prefix()
        {
            // TODO Add Sounds / Fix Sound Bug
            return false;
        }
    }
}
