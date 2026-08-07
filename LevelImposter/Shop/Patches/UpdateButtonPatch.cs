using HarmonyLib;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Shop.Patches;

/*
 *      Adds the update button to
 *      the Main Menu screen
 */
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class UpdateButtonPatch
{
    public static void Postfix()
    {
        // Initialize Mod Updater
        var prefab = PackagedResources.LoadFromBundle<GameObject>("ModUpdater");
        Object.Instantiate(prefab);
    }
}