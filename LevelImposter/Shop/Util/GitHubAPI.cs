using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;

namespace LevelImposter.Shop;

/// <summary>
///     API to recieve updates from GitHub.com
/// </summary>
public static class GitHubAPI
{
    public const string API_PATH = "https://api.github.com/repos/DigiWorm0/LevelImposter/releases?per_page=5";
    public const string UPDATE_FORBIDDEN_FLAG = "[NoAutoUpdate]";
    public const string DEV_VERSION_FLAG = "dev";

    /// <summary>
    ///     Gets the current path where the LevelImposter DLL is stored.
    /// </summary>
    /// <returns>String path where the LevelImposter DLL is stored.</returns>
    public static string GetDLLDirectory()
    {
        var gameDir = Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
        return gameDir;
    }

    /// <summary>
    ///     Gets the latest release info from GitHub
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    private static void GetLatestReleases(Action<GHRelease[]> onSuccess, Action<string> onError)
    {
        LILogger.Info("Getting latest release info from GitHub");
        HTTPHandler.Instance?.Request(API_PATH, json =>
        {
            var response = JsonSerializer.Deserialize<GHRelease[]>(json);
            if (response != null)
                onSuccess(response);
            else
                onError("Invalid API response");
        }, onError);
    }

    /// <summary>
    ///     Gets the latest release data from GitHub
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    public static void GetLatestRelease(Action<GHRelease> onSuccess, Action<string> onError)
    {
        GetLatestReleases(releases => { onSuccess(releases[0]); }, onError);
    }

    /// <summary>
    ///     Checks release chain if update is forbidden
    /// </summary>
    /// <param name="releases">List of releases in order of relevancy</param>
    /// <returns>TRUE if the update is forbidden. FALSE otherwise</returns>
    [HideFromIl2Cpp]
    private static bool IsUpdateForbidden(GHRelease[] releases)
    {
        foreach (var release in releases)
        {
            if (IsCurrent(release))
                return false;
            if (release.Body.Contains(UPDATE_FORBIDDEN_FLAG))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Checks if the GitHub release matches
    ///     the current release version
    /// </summary>
    /// <param name="release">Release data to check</param>
    /// <returns>True if the release matches the current version</returns>
    [HideFromIl2Cpp]
    public static bool IsCurrent(GHRelease release)
    {
        var versionString = release.Name.Split(" ")[1];
        return versionString == LevelImposter.DisplayVersion || LevelImposter.DisplayVersion.Contains(DEV_VERSION_FLAG);
    }

    /// <summary>
    ///     Downloads and updates the DLL to the latest version
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    public static void UpdateMod(Action onSuccess, Action<string> onError)
    {
        LILogger.Info("Updating mod from GitHub");
        GetLatestReleases(releases =>
        {
            var release = releases[0];
            LILogger.Info($"Downloading DLL from {release}");
            if (release.Assets.Length <= 0)
            {
                var errorMsg = $"No release assets were found for {release}";
                LILogger.Error(errorMsg);
                onError(errorMsg);
                return;
            }

            if (IsUpdateForbidden(releases))
            {
                var errorMsg = $"Auto-update to {release} is unavailable.";
                LILogger.Error(errorMsg);
                onError(errorMsg);
                return;
            }

            var downloadURL = release.Assets[0].BrowserDownloadURL;
            HTTPHandler.Instance?.Request(downloadURL, dllBytes =>
            {
                LILogger.Info($"Saving {dllBytes.Length / 1024}kb DLL to local filesystem");
                try
                {
                    var dllPath = GetDLLDirectory();
                    var dllOldPath = dllPath + ".old";

                    if (File.Exists(dllOldPath))
                        File.Delete(dllOldPath);
                    File.Move(dllPath, dllOldPath);

                    using (var fileStream = File.Create(dllPath))
                    {
                        fileStream.Write(dllBytes, 0, dllBytes.Length);
                    }

                    FileCache.Clear();

                    LILogger.Info("Update complete");
                    onSuccess();
                }
                catch (Exception e)
                {
                    LILogger.Error(e);
                    onError(e.Message);
                }
            }, onError);
        }, onError);
    }
}