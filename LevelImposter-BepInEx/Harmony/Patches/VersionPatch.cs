using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class VersionPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.text += "\n<color=#3399FF>Level<color=#FF0000>Imposter<color=#FFFFFF>\nv" + MainHarmony.VERSION + "\nby DigiWorm";
        }
    }
}
