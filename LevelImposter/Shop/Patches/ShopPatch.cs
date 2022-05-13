using HarmonyLib;
using System;
using System.Collections.Generic;
using UnhollowerBaseLib.Attributes;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    [HarmonyPatch(typeof(HowToPlayController), nameof(HowToPlayController.Start))]
    public static class ShopPatch
    {
        public static void Postfix()
        {
            ShopBuilder.OnLoad();
        }
    }
}
