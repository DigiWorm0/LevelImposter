using HarmonyLib;
using LevelImposter.Core.Utils;
using LevelImposter.Networking.API;
using LevelImposter.Shop.Builders;

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
        // Don't check for updates on dev builds
        if (LevelImposter.IsDevBuild)
            return;

        // Check for updates
        GitHubAPI.GetLatestRelease(release =>
        {
            if (!GitHubAPI.IsCurrent(release))
                UpdateButtonBuilder.Build();
        }, error => { LILogger.Warn("Failed to check for updates: " + error); });
    }
}