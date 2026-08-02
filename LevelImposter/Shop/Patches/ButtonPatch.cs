using HarmonyLib;
using LevelImposter.Shop.Builders;

namespace LevelImposter.Shop.Patches;

/*
 *      Replaces the Inventory
 *      Button in the Main Menu
 *      with the Map Shop Button
 */
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
[HarmonyPriority(Priority.First)]
public static class ButtonPatch
{
    public static void Postfix()
    {
        MainMenuBuilder.Build();
    }
}