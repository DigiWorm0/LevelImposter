using HarmonyLib;
using LevelImposter.Map;
using LevelImposter.MinimapGen;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(SystemTypes))]
    public static class TextPatch
    {
        public static bool Prefix(SystemTypes PAECGDHCGJC, ref string __result)
        {
            LILogger.LogInfo("Translation Fix:" + PAECGDHCGJC);
            if (TextHandler.Contains(PAECGDHCGJC))
            {
                __result = TextHandler.Get(PAECGDHCGJC);
                LILogger.LogInfo("Fixed! " + __result);
                return false;
            }
            return true;
        }
    }
}
