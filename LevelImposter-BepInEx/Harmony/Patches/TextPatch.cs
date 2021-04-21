using HarmonyLib;
using LevelImposter.Map;
using LevelImposter.MinimapGen;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString))]
    public static class TextPatch
    {

        public static bool Prefix(SystemTypes PAECGDHCGJC, ref string __result)
        {
            if (TextHandler.Contains(PAECGDHCGJC))
            {
                __result = TextHandler.Get(PAECGDHCGJC);
                return false;
            }

            return true;
        }
    }
}
