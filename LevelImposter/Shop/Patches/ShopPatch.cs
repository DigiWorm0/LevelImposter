using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      Replaces the How To Play
     *      menu with the Map Shop
     */
    [HarmonyPatch(typeof(HowToPlayController), nameof(HowToPlayController.Start))]
    public static class ShopPatch
    {
        public static void Postfix()
        {
            ShopBuilder.OnLoad();
        }
    }
}
