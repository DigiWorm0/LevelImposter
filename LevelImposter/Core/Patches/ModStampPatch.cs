using HarmonyLib;
using System;
using System.Collections.Generic;
using UnhollowerBaseLib.Attributes;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class ModStampPatch
    {
        public static void Postfix()
        {
            ModManager.Instance.ShowModStamp();
        }
    }
}
