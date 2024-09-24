using HarmonyLib;

namespace LevelImposter.Shop;

/*
 *      Replaces the Inventory
 *      Button in the Main Menu
 *      with the Map Shop Button
 */
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class ButtonPatch
{
    public static void Postfix()
    {
        MainMenuBuilder.Build();
    }
}