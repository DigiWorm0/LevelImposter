using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    /*
     *      Adds the update button to
     *      the Main Menu screen
     */
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class UpdateButtonPatch
    {
        public static void Postfix()
        {
            GitHubAPI.GetLatestRelease((release) => {
                if (!GitHubAPI.IsCurrent(release))
                    UpdateButtonBuilder.Build();
            }, (error) => {
                LILogger.Warn("Failed to check for updates: " + error);
            });
        }
    }
}