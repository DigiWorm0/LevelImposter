using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    /*
     *      Replaces the Inventory
     *      menu with the Map Shop
     */
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Start))]
    public static class ShopPatch
    {
        public static bool Prefix(PlayerCustomizationMenu __instance)
        {
            if (GameState.IsInLobby)
                return true;

            UnityEngine.Object.Destroy(__instance.gameObject);
            ShopBuilder.Build();
            return false;
        }
    }
}
