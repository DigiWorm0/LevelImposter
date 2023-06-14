using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      Replaces the Inventory
     *      menu with the Map Shop
     */
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Start))]
    public static class ShopPatch
    {
        public static void Postfix()
        {
            ShopBuilder.OnCustomizationMenu();
        }
    }
}
