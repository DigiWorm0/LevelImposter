using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.text += "\n<color=#3399FF>Level<color=#FF0000>Imposter<color=#FFFFFF> v" + MainHarmony.VERSION;
            __instance.text.transform.position += new Vector3(0, -0.15f, 0);
        }
    }
}
