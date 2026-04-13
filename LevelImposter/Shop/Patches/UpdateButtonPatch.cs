using HarmonyLib;
using LevelImposter.Core;
using LevelImposter.Networking.API;

namespace LevelImposter.Shop;

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
        GitHubAPI.GetLatestRelease(release => {
            if (!GitHubAPI.IsCurrent(release))
                UpdateButtonBuilder.Build();
        }, error => {
            LILogger.Warn("Failed to check for updates: " + error);
        });
    }
}