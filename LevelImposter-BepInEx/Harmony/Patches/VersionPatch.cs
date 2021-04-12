using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.Text += "\n\n\n\n\n\n\n[3399FFFF]Level[FF0000FF]Imposter[] v" + MainHarmony.VERSION + " by DigiWorm";
        }
    }
}
