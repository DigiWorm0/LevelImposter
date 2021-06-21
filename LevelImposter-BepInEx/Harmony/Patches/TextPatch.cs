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
        public static bool Prefix(SystemTypes room, ref string __result)
        {
            if (TextHandler.Contains(room))
            {
                __result = TextHandler.Get(room);
                return false;
            }
            return true;
        }
    }
}
